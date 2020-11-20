using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Storagr.Data
{
    public class SqliteBackendOptions : StoragrOptions<SqliteBackendOptions>
    {
        public string DataSource { get; set; }
    }

    public static class SqliteBackendExtension
    {
        public static IServiceCollection AddSqliteBackend(this IServiceCollection services, Action<SqliteBackendOptions> configureOptions)
        {
            return services
                .AddOptions()
                .Configure(configureOptions)
                .AddSingleton<SqliteBackend>()
                .AddSingleton<IBackendAdapter>(x => x.GetRequiredService<SqliteBackend>());
        }
    }
    
    public class SqliteBackend : IDisposable, IBackendAdapter
    {
        #region Class<QueryBuilder>

        private class QueryBuilder : IBackendQuery
        {
            private string _selector;
            private bool _distinct;
            private int _limit, _offset;
            private string _where;
            private string _orderBy;
            
            public QueryBuilder()
            {
                _distinct = false;
                _selector = "*";
                _limit = -1;
                _offset = -1;
                _where = "";
                _orderBy = "";
            }

            public string Build(string table) // TODO optimize this, maybe List?
            {
                var query = $"SELECT" + (_distinct ? " DISTINCT" : "") + $" {_selector} FROM {table}";

                if (_where.Length > 0)
                {
                    query += $" {_where}";
                }

                if (_limit > 0)
                    query += $" LIMIT {_limit}";

                if (_offset > 0)
                    query += $" OFFSET {_offset}";

                if (_orderBy.Length > 0)
                    query += $" {_orderBy}";

                return query;
            }
            
            public IBackendQuery Select(params string[] columns)
            {
                _selector = columns.Length == 0 ? "*" : string.Join(", ", columns);
                return this;
            }

            public IBackendQuery Distinct()
            {
                _distinct = true;
                return this;
            }

            public IBackendQuery Where(Action<IBackendFilter> filterBuilder)
            {
                var builder = new FilterBuilder();
                
                filterBuilder.Invoke(builder);

                var filter = builder.Build();

                _where = $"WHERE {filter}";
                return this;
            }

            public IBackendQuery Limit(int limit)
            {
                _limit = limit;
                return this;
            }

            public IBackendQuery Offset(int offset)
            {
                _offset = offset;
                return this;
            }

            public IBackendQuery OrderBy(Action<IBackendOrder> orderByBuilder)
            {
                var builder = new OrderBuilder();
                
                orderByBuilder.Invoke(builder);

                var filter = builder.Build();

                _orderBy = $"ORDER BY {filter}";
                return this;
            }
        }

        #endregion

        #region Class<FilterBuilder>

        private class FilterBuilder : IBackendFilter
        {
            private readonly List<string> _list;

            public FilterBuilder()
            {
                _list = new List<string>();
            }

            public string Build()
            {
                return string.Join(' ', _list).Trim();
            }

            private void CheckAndOr()
            {
                if (_list.Count % 2 == 0)
                    throw new Exception("AND|OR already added before.");
            }

            private void CheckWhereStatement()
            {
                if (_list.Count % 2 != 0)
                    throw new Exception("Add AND or OR before `` statement.");
            }
            
            public IBackendFilter Clamp(Action<IBackendFilter> filterBuilder)
            {
                CheckWhereStatement();
                
                var builder = new FilterBuilder();
                
                filterBuilder.Invoke(builder);

                var filter = builder.Build();

                _list.Add($"({filter})");
                return this;
            }

            public IBackendFilter Or()
            {
                CheckAndOr();
                _list.Add("OR");
                return this;
            }

            public IBackendFilter And()
            {
                CheckAndOr();
                _list.Add("AND");
                return this;
            }

            public IBackendFilter Not()
            {
                _list.Add("NOT");
                return this;
            }

            public IBackendFilter Equal(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} = '{value}'");
                return this;
            }

            public IBackendFilter Like(string column, string pattern)
            {
                CheckWhereStatement();
                _list.Add($"{column} LIKE '{pattern}'");
                return this;
            }

            public IBackendFilter In(string column, params string[] values)
            {
                CheckWhereStatement();
                _list.Add($"{column} IN ({string.Join(",", values.Select(v => $"'{v}'"))})");
                return this;
            }

            public IBackendFilter Between(string column, string value1, string value2)
            {
                CheckWhereStatement();
                _list.Add($"{column} BETWEEN '{value1}' AND '{value2}'");
                return this;
            }

            public IBackendFilter GreaterThan(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} > '{value}'");
                return this;
            }

            public IBackendFilter GreaterThanOrEqual(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} >= '{value}'");
                return this;
            }

            public IBackendFilter LessThan(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} < '{value}'");
                return this;
            }

            public IBackendFilter LessThanOrEqual(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} >= '{value}'");
                return this;
            }
        }

        #endregion

        #region Class<OrderBuilder>

        private class OrderBuilder : IBackendOrder
        {
            private readonly List<string> _list;

            public OrderBuilder()
            {
                _list = new List<string>();
            }
            
            public string Build()
            {
                return string.Join(' ', _list).Trim();
            }
            
            public IBackendOrder Column(string name, BackendOrderType type)
            {
                _list.Add($"{name} {(type == BackendOrderType.Asc ? "ASC" : "DESC")}");
                return this;
            }
        }

        #endregion

        #region Fields

        private readonly SqliteBackendOptions _options;
        private readonly IDbConnection _connection;

        #endregion

        #region CTor

        public SqliteBackend(IOptions<SqliteBackendOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            _options = optionsAccessor.Value;
            _connection = new SqliteConnection($"Data Source={_options.DataSource}");
        }

        #endregion

        #region Methods<IBackendAdapter>

        public async Task<T> Get<T>(string id) where T : class
        {
            if ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault() == null)
            {
                throw new Exception($"Type {typeof(T).Name} has no [Table] attribute.");
            }
            return await _connection.GetAsync<T>(id);
        }

        public async Task<T> Get<T>(Action<IBackendQuery> queryBuilder) where T : class
        {
            var table = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault();
            if (table == null)
            {
                throw new Exception($"Type {typeof(T).Name} has no [Table] attribute.");
            }
            
            var builder = new QueryBuilder();
            queryBuilder.Invoke(builder);
            var query = builder.Build(table.Name);
            
            return await _connection.QuerySingleOrDefaultAsync<T>(query);
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : class
        {
            if ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault() == null)
            {
                throw new Exception($"Type {typeof(T).Name} has no [Table] attribute.");
            }
            return await _connection.GetAllAsync<T>();
        }

        public async Task<IEnumerable<T>> GetAll<T>(Action<IBackendQuery> queryBuilder) where T : class
        {
            var table = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault();
            if (table == null)
            {
                throw new Exception("Type T has no [Table] attribute.");
            }

            var builder = new QueryBuilder();
            queryBuilder.Invoke(builder);
            var query = builder.Build(table.Name);
            
            return await _connection.QueryAsync<T>(query);
        }

        public async Task<int> Insert<T>(T data) where T : class
        {
            if ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault() == null)
            {
                throw new Exception($"Type {typeof(T).Name} has no [Table] attribute.");
            }
            return await _connection.InsertAsync<T>(data);
        }

        public async Task Update<T>(T data) where T : class
        {
            if ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault() == null)
            {
                throw new Exception($"Type {typeof(T).Name} has no [Table] attribute.");
            }
            await _connection.UpdateAsync<T>(data);
        }

        public async Task<bool> Delete<T>(T data) where T : class
        {
            if ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault() == null)
            {
                throw new Exception($"Type {typeof(T).Name} has no [Table] attribute.");
            }
            return await _connection.DeleteAsync<T>(data);
        }

        #endregion

        #region Methods<IDisposable>

        public void Dispose()
        {
            _connection?.Dispose();
        }

        #endregion
    }
}