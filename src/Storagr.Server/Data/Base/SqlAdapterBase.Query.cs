using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Storagr.Shared;

namespace Storagr.Server.Data
{
    public partial class SqlAdapterBase
    {
        private class Query<T> :
            IDatabaseQuery<T>,
            IDatabaseQueryList<T>,
            IDatabaseOrder<T>,
            IDatabaseSelector<T>
        {
            private readonly IDbConnection _connection;
            private readonly QueryBuilder<T> _queryBuilder;

            public Query(IDbConnection connection)
            {
                _connection = connection;
                _queryBuilder = new QueryBuilder<T>();
                _queryBuilder.Select<T>("*");
            }

            T IStoragrRunner<T>.Run() => (this as IStoragrRunner<T>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            Task<T> IStoragrRunner<T>.RunAsync(CancellationToken cancellationToken)
            {
                return _connection.QueryFirstOrDefaultAsync<T>(_queryBuilder.Build());
            }

            IEnumerable<T> IStoragrRunner<IEnumerable<T>>.Run() => (this as IStoragrRunner<IEnumerable<T>>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            Task<IEnumerable<T>> IStoragrRunner<IEnumerable<T>>.RunAsync(CancellationToken cancellationToken)
            {
                return _connection.QueryAsync<T>(_queryBuilder.Build());
            }
            
            IDatabaseSelector<T> IDatabaseSelector<T>.Column<TJoinItem>(string column, string alias)
            {
                _queryBuilder.Select<TJoinItem>(column, alias);
                return this;
            }

            public IDatabaseQueryList<T> Distinct()
            {
                _queryBuilder.Distinct();
                return this;
            }

            IDatabaseQuery<T> IDatabaseQuery<T>.With(Action<IDatabaseSelector<T>> select)
            {
                select(this);
                return this;
            }

            IDatabaseQueryList<T> IDatabaseQueryList<T>.With(Action<IDatabaseSelector<T>> select) =>
                (IDatabaseQueryList<T>) (this as IDatabaseQuery<T>).With(select);

            IDatabaseQuery<T> IDatabaseQuery<T>.Join<TJoinItem>(string column, string joinedColumn)
            {
                _queryBuilder.Join<TJoinItem>(column, joinedColumn);
                return this;
            }

            IDatabaseQueryList<T> IDatabaseQueryList<T>.Join<TJoinItem>(string column, string joinedColumn) =>
                (IDatabaseQueryList<T>) (this as IDatabaseQuery<T>).Join<TJoinItem>(column, joinedColumn);

            IDatabaseQuery<T> IDatabaseQuery<T>.Where(Action<IDatabaseFilter<T>> filter)
            {
                _queryBuilder.Where(filter);
                return this;
            }

            IDatabaseQueryList<T> IDatabaseQueryList<T>.Where(Action<IDatabaseFilter<T>> filter) =>
                (IDatabaseQueryList<T>) (this as IDatabaseQuery<T>).Where(filter);

            IDatabaseQueryList<T> IDatabaseQueryList<T>.OrderBy(Action<IDatabaseOrder<T>> order)
            {
                order(this);
                return this;
            }

            IDatabaseQueryList<T> IDatabaseQueryList<T>.Limit(int count)
            {
                _queryBuilder.Limit(count);
                return this;
            }

            IDatabaseQueryList<T> IDatabaseQueryList<T>.Offset(int count)
            {
                _queryBuilder.Offset(count);
                return this;
            }

            IDatabaseOrder<T> IDatabaseOrder<T>.Asc<TJoinItem>(string column)
            {
                _queryBuilder.Order<TJoinItem>(column);
                return this;
            }

            IDatabaseOrder<T> IDatabaseOrder<T>.Desc<TJoinItem>(string column)
            {
                _queryBuilder.Order<TJoinItem>(column, false);
                return this;
            }
        }
    }
}