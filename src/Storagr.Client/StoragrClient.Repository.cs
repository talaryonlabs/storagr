using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IRepositoryCreator : IStoragrCreator<StoragrRepository>
    {
        IRepositoryCreator SetOwner(string owner);
        IRepositoryCreator SetSizeLimit(ulong sizeLimit);
    }
    
    public interface IRepositoryUpdater : IStoragrUpdater<StoragrRepository>
    {
        IRepositoryUpdater SetName(string newName);
        IRepositoryUpdater SetOwner(string newOwner);
        IRepositoryUpdater SetSizeLimit(ulong newSizeLimit);
    }
    
    public partial class StoragrClient
    {
        private class RepositoryCreator : StoragrClientHelper, IRepositoryCreator
        {
            private readonly StoragrRepository _repository;
            
            public RepositoryCreator(string name, StoragrClient storagrClient, HttpClient httpClient)
                : base(storagrClient, httpClient)
            {
                _repository = new StoragrRepository()
                {
                    Name = name
                };
            }

            public IRepositoryCreator SetOwner(string owner)
            {
                _repository.OwnerId = owner;
                return this;
            }

            public IRepositoryCreator SetSizeLimit(ulong sizeLimit)
            {
                _repository.SizeLimit = sizeLimit;
                return this;
            }
            
            public async Task<StoragrRepository> Create(CancellationToken cancellationToken = default)
            {
                var request = StoragrClient.CreateRequest($"repositories", HttpMethod.Post, _repository);
                var response = await HttpClient.SendAsync(request, cancellationToken);
                var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw (StoragrError) data;

                return data;
            }
        }
        
        private class RepositoryUpdater : StoragrClientHelper, IRepositoryUpdater
        {
            private readonly string _repositoryId;
            private readonly StoragrUpdateRequest _repositoryUpdateRequest;

            public RepositoryUpdater(string repositoryId, StoragrClient storagrClient, HttpClient httpClient)
                : base(storagrClient, httpClient)
            {
                _repositoryId = repositoryId;
                _repositoryUpdateRequest = new StoragrUpdateRequest()
                {
                    Type = StoragrUpdateType.Repository
                };
            }

            public IRepositoryUpdater SetName(string newName)
            {
                _repositoryUpdateRequest.Updates.Add("name", newName);
                return this;
            }

            public IRepositoryUpdater SetOwner(string newOwner)
            {
                _repositoryUpdateRequest.Updates.Add("owner", newOwner);
                return this;
            }

            public IRepositoryUpdater SetSizeLimit(ulong newSizeLimit)
            {
                _repositoryUpdateRequest.Updates.Add("size_limit", newSizeLimit);
                return this;
            }

            public async Task<StoragrRepository> Update(CancellationToken cancellationToken = default)
            {
                var request = StoragrClient.CreateRequest($"repositories/{_repositoryId}", HttpMethod.Patch,
                    _repositoryUpdateRequest);
                var response = await HttpClient.SendAsync(request, cancellationToken);
                var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw (StoragrError) data;

                return data;
            }
        }
    }
}