using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientObject : StoragrClientHelper, IStoragrClientObject
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

        StoragrObject IStoragrClientRunner<StoragrObject>.Run()
        {
            var task = (this as IStoragrClientObject).RunAsync();
            task.RunSynchronously();
            return task.Result;
        }

        Task<StoragrObject> IStoragrClientRunner<StoragrObject>.RunAsync(CancellationToken cancellationToken)
        {
            return Request<StoragrObject>(
                $"repositories/{_repositoryIdOrName}/objects/{_objectId}",
                _deleteRequest
                    ? HttpMethod.Delete
                    : HttpMethod.Get,
                cancellationToken);
        }

        IStoragrClientRunner<StoragrObject> IStoragrClientDeletable<StoragrObject>.Delete()
        {
            _deleteRequest = true;
            return this;
        }
    }
}