using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Data;

namespace Storagr.Client
{
    internal class StoragrClientRepository : 
        StoragrClientHelper<StoragrRepository>, 
        IStoragrParams<StoragrRepository, IStoragrRepositoryParams>, 
        IStoragrRepository, 
        IStoragrRepositoryParams
    {
        private readonly IStoragrClientRequest _clientRequest;
        private readonly string _repositoryIdOrName;
        
        private bool _deleteRequest;
        private StoragrRequest<StoragrRepository> _createRequest;
        private StoragrRequest<StoragrRepository> _updateRequest;
        
        public StoragrClientRepository(IStoragrClientRequest clientRequest, string repositoryIdOrName) 
            : base(clientRequest)
        {
            _clientRequest = clientRequest;
            _repositoryIdOrName = repositoryIdOrName;
        }

        protected override Task<StoragrRepository> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            if (_createRequest is not null)
                return clientRequest.Send<StoragrRepository, StoragrRequest<StoragrRepository>>(
                    $"repositories",
                    HttpMethod.Post,
                    _createRequest,
                    cancellationToken
                );

            if (_updateRequest is not null)
                return clientRequest.Send<StoragrRepository, StoragrRequest<StoragrRepository>>(
                    $"repositories/{_repositoryIdOrName}",
                    HttpMethod.Patch,
                    _updateRequest,
                    cancellationToken
                );

            return clientRequest.Send<StoragrRepository>(
                $"repositories/{_repositoryIdOrName}",
                _deleteRequest
                    ? HttpMethod.Delete
                    : HttpMethod.Get,
                cancellationToken);
        }

        IStoragrRunner<StoragrRepository> IStoragrParams<StoragrRepository, IStoragrRepositoryParams>.With(Action<IStoragrRepositoryParams> withParams)
        {
            withParams(this);
            return this;
        }

        IStoragrParams<StoragrRepository, IStoragrRepositoryParams> IStoragrCreatable<StoragrRepository, IStoragrRepositoryParams>.Create()
        {
            _createRequest = new StoragrRequest<StoragrRepository>();
            return this;
        }

        IStoragrParams<StoragrRepository, IStoragrRepositoryParams> IStoragrUpdatable<StoragrRepository, IStoragrRepositoryParams>.Update()
        {
            _updateRequest = new StoragrRequest<StoragrRepository>();
            return this;
        }

        IStoragrRunner<StoragrRepository> IStoragrDeletable<StoragrRepository>.Delete(bool force)
        {
            _deleteRequest = true;
            return this;
        }

        IStoragrObject IStoragrObjectProvider.Object(string objectId)
        {
            return new StoragrClientObject(_clientRequest, _repositoryIdOrName, objectId);
        }

        IStoragrClientObjectList IStoragrObjectProvider.Objects()
        {
            return new StoragrClientObjectList(_clientRequest, _repositoryIdOrName);
        }

        IStoragrLock IStoragrLockProvider.Lock(string lockIdOrPath)
        {
            return new StoragrClientLock(_clientRequest, _repositoryIdOrName, lockIdOrPath);
        }

        IStoragrClientLockList IStoragrLockProvider.Locks()
        {
            return new StoragrClientLockList(_clientRequest, _repositoryIdOrName);
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.Id(string repositoryId)
        {
            // skipping - you cannot create or update RepositoryId
            return this;
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.Name(string name)
        {
            (_createRequest ?? _updateRequest).Items.Add("name", name);
            return this;
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.Owner(string owner)
        {
            (_createRequest ?? _updateRequest).Items.Add("owner", owner);
            return this;
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.SizeLimit(ulong sizeLimit)
        {
            (_createRequest ?? _updateRequest).Items.Add("size_limit", sizeLimit);
            return this;
        }
    }
}