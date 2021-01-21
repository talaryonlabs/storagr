using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Storagr.Data.Builders;

namespace Storagr.Data
{
    public abstract class SqlAdapterBase : IDatabaseAdapter
    {
        private IDbConnection _connection;

        public IDatabaseAdapter UseConnection(IDbConnection connection)
        {
            _connection = connection;
            return this;
        }

        public async Task<int> Count<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken)
            where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return 0;

            var table = EntityHelper.GetTableName<T>();
            var query = new SqlQueryBuilder();

            query.Select("COUNT(*)");

            if (filterBuilder is not null)
                query.Where(filterBuilder);

            return await _connection.QuerySingleAsync<int>(query.Build(table));
        }

        public async Task<bool> Exists<T>(string id, CancellationToken cancellationToken) where T : class =>
            await Get<T>(id, cancellationToken) is not null;

        public async Task<bool> Exists<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken)
            where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            var table = EntityHelper.GetTableName<T>();
            var query = new SqlQueryBuilder();
            query.Where(filterBuilder);

            return await _connection.QueryFirstOrDefaultAsync<T>(query.Build(table)) is not null;
        }

        public async Task<T> Get<T>(string id, CancellationToken cancellationToken) where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            EntityHelper.CheckTableAttribute<T>();
            return await _connection.GetAsync<T>(id);
        }

        public async Task<T> Get<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken = default)
            where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var table = EntityHelper.GetTableName<T>();
            var query = new SqlQueryBuilder();
            query.Where(filterBuilder);

            return await _connection.QueryFirstOrDefaultAsync<T>(query.Build(table));
        }

        public async Task<T> Get<T>(Action<IDatabaseQuery> queryBuilder, CancellationToken cancellationToken) where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var table = EntityHelper.GetTableName<T>();
            var query = new SqlQueryBuilder();
            queryBuilder.Invoke(query);

            return await _connection.QueryFirstOrDefaultAsync<T>(query.Build(table));
        }

        public async Task<IEnumerable<T>> GetMany<T>(Action<IDatabaseQuery> queryBuilder, CancellationToken cancellationToken)
            where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var table = EntityHelper.GetTableName<T>();
            var query = new SqlQueryBuilder();
            queryBuilder.Invoke(query);

            return await _connection.QueryAsync<T>(query.Build(table));
        }

        public async Task<IEnumerable<T>> GetMany<T>(Action<IDatabaseFilter> filterBuilder,
            CancellationToken cancellationToken = default) where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var table = EntityHelper.GetTableName<T>();
            var query = new SqlQueryBuilder();
            query.Where(filterBuilder);

            return await _connection.QueryAsync<T>(query.Build(table));
        }

        public async Task<IEnumerable<T>> GetAll<T>(CancellationToken cancellationToken) where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            EntityHelper.CheckTableAttribute<T>();
            return await _connection.GetAllAsync<T>();
        }

        public async Task<int> Insert<T>(T data, CancellationToken cancellationToken) where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return 0;

            EntityHelper.CheckTableAttribute<T>();
            return await _connection.InsertAsync<T>(data);
        }

        public async Task Update<T>(T data, CancellationToken cancellationToken) where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            EntityHelper.CheckTableAttribute<T>();
            await _connection.UpdateAsync<T>(data);
        }

        public async Task<bool> Delete<T>(T data, CancellationToken cancellationToken) where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            EntityHelper.CheckTableAttribute<T>();
            return await _connection.DeleteAsync(data);
        }

        public async Task<bool> Delete<T>(IEnumerable<T> list, CancellationToken cancellationToken) where T : class
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            EntityHelper.CheckTableAttribute<T>();
            return await _connection.DeleteAsync(list);
        }
    }
}