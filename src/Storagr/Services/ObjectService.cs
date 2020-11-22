using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.IO;
using Storagr.Shared;

namespace Storagr.Services
{
    public interface IObjectService
    {
        Task<ObjectEntity> Create(string repositoryId, string objectId, long size);

        Task<RepositoryEntity> Get(string repositoryId);
        Task<ObjectEntity> Get(string repositoryId, string objectId);
        Task<IEnumerable<ObjectEntity>> GetMany(string repositoryId, params string[] objectIds);
        Task<IEnumerable<ObjectEntity>> GetAll(string repositoryId);

        Task Delete(string repositoryId);
        Task Delete(string repositoryId, string objectId);

        Task<StoragrAction> NewVerifyRequest(string repositoryId, string objectId);
        Task<StoragrAction> NewUploadRequest(string repositoryId, string objectId);
        Task<StoragrAction> NewDownloadRequest(string repositoryId, string objectId);
    }
    
    public class ObjectService : IObjectService
    {
        private readonly IBackendAdapter _backendAdapter;
        private readonly IStoreAdapter _storeAdapter;
        private readonly IUserService _userService;

        public ObjectService(IBackendAdapter backendAdapter, IStoreAdapter storeAdapter, IUserService userService)
        {
            _backendAdapter = backendAdapter;
            _storeAdapter = storeAdapter;
            _userService = userService;
        }

        public async Task<ObjectEntity> Create(string repositoryId, string objectId, long size)
        {
            var entity = await Get(repositoryId, objectId);
            if (entity != null)
                throw null; // TODO ObjectAlreadyExistsException

            await _backendAdapter.Insert(entity = new ObjectEntity()
            {
                RepositoryId = repositoryId,
                ObjectId = objectId,
                Size = size
            });
            return entity;
        }

        public Task<RepositoryEntity> Get(string repositoryId) => _backendAdapter.Get<RepositoryEntity>(repositoryId);

        public Task<ObjectEntity> Get(string repositoryId, string objectId)
        {
            return _backendAdapter.Get<ObjectEntity>(q =>
            {
                q.Where(f =>
                {
                    f.Equal("RepositoryId", repositoryId)
                        .And()
                        .Equal("ObjectId", objectId);
                });
            });
        }

        public Task<IEnumerable<ObjectEntity>> GetMany(string repositoryId, params string[] objectIds)
        {
            return _backendAdapter.GetAll<ObjectEntity>(q =>
            {
                q.Where(f =>
                {
                    f.Equal("RepositoryId", repositoryId)
                        .And()
                        .In("ObjectId", objectIds);
                });
            });
        }

        public Task<IEnumerable<ObjectEntity>> GetAll(string repositoryId)
        {
            return _backendAdapter.GetAll<ObjectEntity>(q =>
            {
                q.Where(f =>
                {
                    f.Equal("RepositoryId", repositoryId);
                });
            });
        }

        public async Task Delete(string repositoryId)
        {
            var entity = await Get(repositoryId);
            if (entity == null)
            {
                throw new StoragrRepositoryNotFoundException();
            }
            await _backendAdapter.Delete(entity);
            await _storeAdapter.Delete(repositoryId);
        }

        public async Task Delete(string repositoryId, string objectId)
        {
            var entity = await Get(repositoryId, objectId);
            
            if (entity == null)
            {
                throw null; // TODO ObjectNotFoundException
            }
            await _backendAdapter.Delete(entity);
            await _storeAdapter.Delete(repositoryId, objectId);
        }

        public async Task<StoragrAction> NewVerifyRequest(string repositoryId, string objectId)
        {
            var obj = await Get(repositoryId, objectId);
            if (obj != null) 
                return null;
            
            var token = await _userService.GetAuthenticatedUserToken();
            return new StoragrAction
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 0,
                Href = $"{repositoryId}/objects/v/{objectId}"
            };
        }

        public Task<StoragrAction> NewUploadRequest(string repositoryId, string objectId)
        {
            return _storeAdapter.NewUploadRequest(repositoryId, objectId);
        }

        public Task<StoragrAction> NewDownloadRequest(string repositoryId, string objectId)
        {
            return _storeAdapter.NewDownloadRequest(repositoryId, objectId);
        }
    }
}