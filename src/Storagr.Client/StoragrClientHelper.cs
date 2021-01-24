using System.Threading;
using System.Threading.Tasks;
using Storagr;

namespace Storagr.Client
{
    internal abstract class StoragrClientHelper<T> : IStoragrRunner<T>
    {
        private readonly IStoragrClientRequest _clientRequest;

        protected StoragrClientHelper(IStoragrClientRequest clientRequest)
        {
            _clientRequest = clientRequest;
        }

        protected abstract Task<T> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default);
        
        public T Run()
        {
            var task = (this as IStoragrRunner<T>).RunAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public Task<T> RunAsync(CancellationToken cancellationToken = default)
        {
            return RunAsync(_clientRequest, cancellationToken);
        }
    }
}