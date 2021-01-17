using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientRepository : 
        StoragrClientHelper<StoragrRepository>, 
        IStoragrClientParams<StoragrRepository, IStoragrRepositoryParams>, 
        IStoragrClientRepository, 
        IStoragrRepositoryParams
    {
        private readonly IStoragrClientRequest _clientRequest;
        private readonly string _repositoryIdOrName;
        
        private bool _deleteRequest;
        private StoragrUpdateRequest _createRequest;
        private StoragrUpdateRequest _updateRequest;
        
        public StoragrClientRepository(IStoragrClientRequest clientRequest, string repositoryIdOrName) 
            : base(clientRequest)
        {
            _clientRequest = clientRequest;
            _repositoryIdOrName = repositoryIdOrName;
        }

        protected override Task<StoragrRepository> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            if (_createRequest is not null)
                return clientRequest.Send<StoragrRepository, StoragrUpdateRequest>(
                    $"repositories",
                    HttpMethod.Post,
                    _createRequest,
                    cancellationToken
                );

            if (_updateRequest is not null)
                return clientRequest.Send<StoragrRepository, StoragrUpdateRequest>(
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

        IStoragrClientRunner<StoragrRepository> IStoragrClientParams<StoragrRepository, IStoragrRepositoryParams>.With(Action<IStoragrRepositoryParams> withParams)
        {
            withParams(this);
            return this;
        }

        IStoragrClientParams<StoragrRepository, IStoragrRepositoryParams> IStoragrClientCreatable<StoragrRepository, IStoragrRepositoryParams>.Create()
        {
            _createRequest = new StoragrUpdateRequest();
            return this;
        }

        IStoragrClientParams<StoragrRepository, IStoragrRepositoryParams> IStoragrClientUpdatable<StoragrRepository, IStoragrRepositoryParams>.Update()
        {
            _updateRequest = new StoragrUpdateRequest();
            return this;
        }

        IStoragrClientRunner<StoragrRepository> IStoragrClientDeletable<StoragrRepository>.Delete(bool force)
        {
            _deleteRequest = true;
            return this;
        }

        IStoragrClientObject IStoragrClientRepository.Object(string objectId)
        {
            return new StoragrClientObject(_clientRequest, _repositoryIdOrName, objectId);
        }

        IStoragrClientObjectList IStoragrClientRepository.Objects()
        {
            return new StoragrClientObjectList(_clientRequest, _repositoryIdOrName);
        }

        IStoragrClientLock IStoragrClientRepository.Lock(string lockIdOrPath)
        {
            return new StoragrClientLock(_clientRequest, _repositoryIdOrName, lockIdOrPath);
        }

        IStoragrClientLockList IStoragrClientRepository.Locks()
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
            (_createRequest ?? _updateRequest).Updates.Add("name", name);
            return this;
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.Owner(string owner)
        {
            (_createRequest ?? _updateRequest).Updates.Add("owner", owner);
            return this;
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.SizeLimit(ulong sizeLimit)
        {
            (_createRequest ?? _updateRequest).Updates.Add("size_limit", sizeLimit);
            return this;
        }
    }
}