using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Storagr.Shared;

namespace Storagr.Server.Data
{
    public partial class SqlAdapterBase
    {
        private class Count<T> :
            IDatabaseCount<T>
        {
            private readonly IDbConnection _connection;
            private readonly QueryBuilder<T> _queryBuilder;

            public Count(IDbConnection connection)
            {
                _connection = connection;
                _queryBuilder = new QueryBuilder<T>();
                _queryBuilder.Select("COUNT(*)");
            }

            public IDatabaseCount<T> Join<TJoinItem>(string column, string joinedColumn)
            {
                _queryBuilder.Join<TJoinItem>(column, joinedColumn);
                return this;
            }

            IDatabaseCount<T> IDatabaseCount<T>.Where(Action<IDatabaseFilter<T>> filter)
            {
                _queryBuilder.Where(filter);
                return this;
            }

            int IStoragrRunner<int>.Run() => (this as IStoragrRunner<int>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            Task<int> IStoragrRunner<int>.RunAsync(CancellationToken cancellationToken) =>
                _connection.QuerySingleAsync<int>(_queryBuilder.Build());
        }
    }
}