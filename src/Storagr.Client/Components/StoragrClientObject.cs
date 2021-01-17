using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientObject : 
        StoragrClientHelper<StoragrObject>, 
        IStoragrClientObject
    {
        private readonly string _repositoryIdOrName;
        private readonly string _objectId;

        private bool _deleteRequest;

        public StoragrClientObject(IStoragrClientRequest clientRequest, string repositoryIdOrName, string objectId) 
            : base(clientRequest)
        {
            _repositoryIdOrName = repositoryIdOrName;
            _objectId = objectId;
        }

        protected override Task<StoragrObject> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            return clientRequest.Send<StoragrObject>(
                $"repositories/{_repositoryIdOrName}/objects/{_objectId}",
                _deleteRequest
                    ? HttpMethod.Delete
                    : HttpMethod.Get,
                cancellationToken);
        }

        IStoragrClientRunner<StoragrObject> IStoragrClientDeletable<StoragrObject>.Delete(bool force)
        {
            _deleteRequest = true;
            return this;
        }
    }
}