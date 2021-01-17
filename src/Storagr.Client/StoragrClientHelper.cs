using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Storagr.Client
{
    internal abstract class StoragrClientHelper
    {
        protected IStoragrClientRequest ClientRequest { get; }

        protected StoragrClientHelper(IStoragrClientRequest clientRequest)
        {
            ClientRequest = clientRequest;
        }

        protected Task<TResult> Request<TResult>(string uri, HttpMethod method, CancellationToken cancellationToken = default) =>
            ClientRequest.Send<TResult>(uri, method, cancellationToken);

        protected Task<TResult> Request<TResult, TData>(string uri, HttpMethod method, TData data,
            CancellationToken cancellationToken = default) =>
            ClientRequest.Send<TResult, TData>(uri, method, data, cancellationToken);
    }
}