using System.Threading;
using System.Threading.Tasks;

namespace Storagr.Client
{
    internal abstract class StoragrClientHelper<T> : IStoragrClientRunner<T>
    {
        private readonly IStoragrClientRequest _clientRequest;

        protected StoragrClientHelper(IStoragrClientRequest clientRequest)
        {
            _clientRequest = clientRequest;
        }

        protected abstract Task<T> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default);
        
        public T Run()
        {
            var task = (this as IStoragrClientRunner<T>).RunAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public Task<T> RunAsync(CancellationToken cancellationToken = default)
        {
            return RunAsync(_clientRequest, cancellationToken);
        }
    }
}