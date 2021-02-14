using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientObject : 
        IStoragrClientObject,
        IStoragrRunner<bool>
    {
        private readonly IStoragrClientRequest _clientRequest;
        private readonly string _repositoryIdOrName;
        private readonly string _objectId;

        private bool _deleteRequest;

        public StoragrClientObject(IStoragrClientRequest clientRequest, string repositoryIdOrName, string objectId) 
        {
            _clientRequest = clientRequest;
            _repositoryIdOrName = repositoryIdOrName;
            _objectId = objectId;
        }

        StoragrObject IStoragrRunner<StoragrObject>.Run() => (this as IStoragrRunner<StoragrObject>)
            .RunAsync()
            .RunSynchronouslyWithResult();
        
        Task<StoragrObject> IStoragrRunner<StoragrObject>.RunAsync(CancellationToken cancellationToken)
        {
            return _clientRequest.Send<StoragrObject>(
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
        
        public IStoragrRunner<bool> Exists() => this;

        bool IStoragrRunner<bool>.Run() => (this as IStoragrRunner<bool>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        async Task<bool> IStoragrRunner<bool>.RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await (this as IStoragrRunner<StoragrObject>).RunAsync(cancellationToken);
            }
            catch (UserNotFoundError)
            {
                return false;
            }

            return true;
        }
    }
}