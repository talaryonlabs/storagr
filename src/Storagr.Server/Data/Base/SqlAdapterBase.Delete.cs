using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Storagr.Shared;

namespace Storagr.Server.Data
{
    public partial class SqlAdapterBase
    {
        private class Delete<T> :  
            IStoragrRunner<T> where T : class
        {
            private readonly IDbConnection _connection;
            private readonly T _entity;

            public Delete(IDbConnection connection, T entity)
            {
                _connection = connection;
                _entity = entity;
            }

            T IStoragrRunner<T>.Run() => (this as IStoragrRunner<T>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<T> IStoragrRunner<T>.RunAsync(CancellationToken cancellationToken)
            {
                return await _connection.DeleteAsync(_entity) ? _entity : null;
            }
        }
    }
}