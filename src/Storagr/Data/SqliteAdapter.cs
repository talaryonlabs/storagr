using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Storagr.Shared;

namespace Storagr.Data
{
    [StoragrConfig("Sqlite")]
    public class SqliteOptions : StoragrOptions<SqliteOptions>
    {
        [StoragrConfigValue] public string DataSource { get; set; }
    }

    public class SqliteAdapter : IDisposable, IDatabaseAdapter
    {
        #region Class<QueryBuilder>

        private class QueryBuilder : IDatabaseQuery
        {
            private string _selector;
            private bool _distinct;
            private int _limit, _offset;
            private string _orderBy;

            private FilterBuilder _filter;
            
            public QueryBuilder()
            {
                _selector = "*";
                _limit = -1;
                _offset = -1;
                _filter = new FilterBuilder();
            }

            public string Build(string table)
            {
                var query = new List<string>(new[] {"SELECT", _selector, "FROM", table});
                
                if(_distinct)
                    query.Insert(1, "DISTINCT");

                if (_filter.HasItems)
                    query.AddRange(new[] {"WHERE", _filter.Build()});

                if (!string.IsNullOrEmpty(_orderBy))
                    query.AddRange(new []{"ORDER BY", _orderBy});
                
                if (_limit > 0)
                    query.AddRange(new[] {"LIMIT", _limit.ToString()});

                if (_offset > 0)
                    query.AddRange(new[] {"OFFSET", _offset.ToString()});

                return string.Join(" ", query);
            }
            
            public IDatabaseQuery Select(params string[] columns)
            {
                _selector = columns.Length == 0 ? "*" : string.Join(", ", columns);
                return this;
            }

            public IDatabaseQuery Distinct()
            {
                _distinct = true;
                return this;
            }

            public IDatabaseQuery Where(Action<IDatabaseFilter> filterBuilder)
            {
                _filter = new FilterBuilder();
                
                filterBuilder?.Invoke(_filter);

                return this;
            }

            public IDatabaseQuery Limit(int limit)
            {
                _limit = limit;
                return this;
            }

            public IDatabaseQuery Offset(int offset)
            {
                _offset = offset;
                return this;
            }

            public IDatabaseQuery OrderBy(Action<IDatabaseOrder> orderByBuilder)
            {
                var builder = new OrderBuilder();
                
                orderByBuilder?.Invoke(builder);

                _orderBy = builder.Build();
                return this;
            }
        }

        #endregion

        #region Class<FilterBuilder>

        private class FilterBuilder : IDatabaseFilter
        {
            public bool HasItems => _list?.Count > 0;

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
            
            public IDatabaseFilter Clamp(Action<IDatabaseFilter> filterBuilder)
            {
                CheckWhereStatement();
                if (filterBuilder is null) 
                    return this;
                
                
                var builder = new FilterBuilder();
                filterBuilder.Invoke(builder);
                _list.Add($"({builder.Build()})");
                
                return this;
            }

            public IDatabaseFilter Or()
            {
                CheckAndOr();
                _list.Add("OR");
                return this;
            }

            public IDatabaseFilter And()
            {
                CheckAndOr();
                _list.Add("AND");
                return this;
            }

            public IDatabaseFilter Not()
            {
                _list.Add("NOT");
                return this;
            }

            public IDatabaseFilter Equal(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} = '{value}'");
                return this;
            }

            public IDatabaseFilter Like(string column, string pattern)
            {
                CheckWhereStatement();
                _list.Add($"{column} LIKE '{pattern}'");
                return this;
            }

            public IDatabaseFilter In(string column, IEnumerable<string> values)
            {
                CheckWhereStatement();
                _list.Add($"{column} IN ({string.Join(",", values.Select(v => $"'{v}'"))})");
                return this;
            }

            public IDatabaseFilter Between(string column, string value1, string value2)
            {
                CheckWhereStatement();
                _list.Add($"{column} BETWEEN '{value1}' AND '{value2}'");
                return this;
            }

            public IDatabaseFilter GreaterThan(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} > '{value}'");
                return this;
            }

            public IDatabaseFilter GreaterThanOrEqual(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} >= '{value}'");
                return this;
            }

            public IDatabaseFilter LessThan(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} < '{value}'");
                return this;
            }

            public IDatabaseFilter LessThanOrEqual(string column, string value)
            {
                CheckWhereStatement();
                _list.Add($"{column} >= '{value}'");
                return this;
            }
        }

        #endregion

        #region Class<OrderBuilder>

        private class OrderBuilder : IDatabaseOrder
        {
            private readonly List<string> _list;

            public OrderBuilder()
            {
                _list = new List<string>();
            }
            
            public string Build()
            {
                return string.Join(", ", _list).Trim();
            }
            
            public IDatabaseOrder Column(string name, DatabaseOrderType type)
            {
                _list.Add($"{name} {(type == DatabaseOrderType.Asc ? "ASC" : "DESC")}");
                return this;
            }
        }

        #endregion

        #region Fields

        private readonly IDbConnection _connection;

        #endregion

        #region CTor

        public SqliteAdapter(IOptions<SqliteOptions> optionsAccessor)
        {
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            _connection = new SqliteConnection($"Data Source={optionsAccessor.Value.DataSource}");
        }

        #endregion

        private static string GetTableName<T>()
        {
            CheckTableAttribute<T>();
            return typeof(T).GetCustomAttribute<TableAttribute>()?.Name;
        }

        private static void CheckTableAttribute<T>()
        {
            if ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault() is null)
                throw new Exception($"Type {typeof(T).Name} has no [Table] attribute.");
        }
        
        #region Methods<IBackendAdapter>

        public Task<int> Count<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken) where T : class
        {
            var table = GetTableName<T>();
            var query = new QueryBuilder();
            
            query.Select("COUNT(*)");
            
            if(filterBuilder is not null)
                query.Where(filterBuilder);

            return Task.Run(() => _connection.QueryFirstAsync<int>(query.Build(table)), cancellationToken);
        }

        public async Task<bool> Exists<T>(string id, CancellationToken cancellationToken) where T : class =>
            await Get<T>(id, cancellationToken) is not null;

        public async Task<bool> Exists<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken) where T : class
        {
            var table = GetTableName<T>();
            var query = new QueryBuilder();
            query.Where(filterBuilder);

            return await Task.Run(() => _connection.QueryFirstAsync<T>(query.Build(table)), cancellationToken) is not null;
        }

        public Task<T> Get<T>(string id, CancellationToken cancellationToken) where T : class
        {
            CheckTableAttribute<T>();
            return Task.Run(() => _connection.GetAsync<T>(id), cancellationToken);
        }

        public Task<T> Get<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken = default) where T : class
        {
            var table = GetTableName<T>();
            var query = new QueryBuilder();
            query.Where(filterBuilder);

            return Task.Run(() => _connection.QueryFirstAsync<T>(query.Build(table)), cancellationToken);
        }

        public Task<T> Get<T>(Action<IDatabaseQuery> queryBuilder, CancellationToken cancellationToken) where T : class
        {
            var table = GetTableName<T>();
            var query = new QueryBuilder();
            queryBuilder.Invoke(query);

            return Task.Run(() => _connection.QuerySingleOrDefaultAsync<T>(query.Build(table)), cancellationToken);
        }

        public Task<IEnumerable<T>> GetMany<T>(Action<IDatabaseQuery> queryBuilder, CancellationToken cancellationToken) where T : class
        {
            var table = GetTableName<T>();
            var query = new QueryBuilder();
            queryBuilder.Invoke(query);

            return Task.Run(() => _connection.QueryAsync<T>(query.Build(table)), cancellationToken);
        }

        public Task<IEnumerable<T>> GetMany<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken = default) where T : class
        {
            var table = GetTableName<T>();
            var query = new QueryBuilder();
            query.Where(filterBuilder);

            return Task.Run(() => _connection.QueryAsync<T>(query.Build(table)), cancellationToken);
        }

        public Task<IEnumerable<T>> GetAll<T>(CancellationToken cancellationToken) where T : class
        {
            CheckTableAttribute<T>();
            return Task.Run(() => _connection.GetAllAsync<T>(), cancellationToken);
        }

        public Task<int> Insert<T>(T data, CancellationToken cancellationToken) where T : class
        {
            CheckTableAttribute<T>();
            return Task.Run(() => _connection.InsertAsync<T>(data), cancellationToken);
        }

        public Task Update<T>(T data, CancellationToken cancellationToken) where T : class
        {
            CheckTableAttribute<T>();
            return Task.Run(() => _connection.UpdateAsync<T>(data), cancellationToken);
        }

        public Task<bool> Delete<T>(T data, CancellationToken cancellationToken) where T : class
        {
            CheckTableAttribute<T>();
            return Task.Run(() => _connection.DeleteAsync(data), cancellationToken);
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