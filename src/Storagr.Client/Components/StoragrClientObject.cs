using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Data;

namespace Storagr.Client
{
    internal class StoragrClientObject : 
        StoragrClientHelper<StoragrObject>, 
        IStoragrObject
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

        IStoragrRunner<StoragrObject> IStoragrDeletable<StoragrObject>.Delete(bool force)
        {
            _deleteRequest = true;
            return this;
        }
    }
}