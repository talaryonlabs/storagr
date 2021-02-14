using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Storagr.Shared;

namespace Storagr.Server.Data
{
    public partial class SqlAdapterBase
    {
        private class Insert<T> :  
            IStoragrRunner<T> where T : class
        {
            private readonly IDbConnection _connection;
            private readonly T _entity;

            public Insert(IDbConnection connection, T entity)
            {
                _connection = connection;
                _entity = entity;
            }

            T IStoragrRunner<T>.Run() => (this as IStoragrRunner<T>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<T> IStoragrRunner<T>.RunAsync(CancellationToken cancellationToken)
            {
                await _connection.InsertAsync(_entity);
                return _entity;
            }
        }
    }
}