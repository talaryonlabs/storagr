using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Services
{
    public class ObjectService : IObjectService
    {
        private readonly IBackendAdapter _backend;
        private readonly IStoreAdapter _store;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;

        public ObjectService(IBackendAdapter backend, IStoreAdapter store, IUserService userService, ICacheService cacheService)
        {
            _backend = backend;
            _store = store;
            _userService = userService;
            _cacheService = cacheService;
        }

        public async Task<int> Count(string repositoryId)
        {
            // var cacheKey = $"STORAGR:REPOSITORY:{repositoryId}:OBJECT:COUNT";
            //
            // _cacheService.Count<RepositoryEntity, ObjectEntity>(repositoryId)
            //
            // if (await _cache.ExistsAsync(cacheKey))
            // {
            //     return await _cache.GetAsyncObject<int>(cacheKey);
            // }
            
            var count = await _backend.Count<ObjectEntity>(filter =>
            {
                filter.Equal(nameof(ObjectEntity.RepositoryId), repositoryId);
            });
            // await _cache.SetAsyncObject(cacheKey, count, _cacheEntryOptions);
            
            return count;
        }


        public async Task<bool> Exists(string repositoryId, string objectId)
        {
            // var cacheKey = $"STORAGR:REPOSITORY:{repositoryId}:OBJECT:{objectId}";
            //
            // if (await _cache.ExistsAsync(cacheKey))
            // {
            //     
            // }
            
            return await _backend.Exists<ObjectEntity>(filter => filter
                .Equal(nameof(ObjectEntity.RepositoryId), repositoryId)
                .And()
                .Equal(nameof(ObjectEntity.Id), objectId)
            );
        }
            

        public async Task<ObjectEntity> Get(string repositoryId, string objectId) =>
            await _backend.Get<ObjectEntity>(query =>
            {
                query.Where(filter => filter
                    .Equal(nameof(ObjectEntity.RepositoryId), repositoryId)
                    .And()
                    .Equal(nameof(ObjectEntity.Id), objectId)
                );
            }) ?? throw new ObjectNotFoundError();

        public Task<IEnumerable<ObjectEntity>> GetMany(string repositoryId, IEnumerable<string> objectIds) =>
            _backend.GetAll<ObjectEntity>(query =>
            {
                query.Where(filter => filter
                    .Equal(nameof(ObjectEntity.RepositoryId), repositoryId)
                    .And()
                    .In(nameof(ObjectEntity.Id), objectIds)
                );
            });
        
        public Task<IEnumerable<ObjectEntity>> GetAll(string repositoryId) =>
            _backend.GetAll<ObjectEntity>(query =>
            {
                query.Where(filter => filter
                    .Equal(nameof(ObjectEntity.RepositoryId), repositoryId)
                );
            });

        public async Task Create(string repositoryId, ObjectEntity entity)
        {
            if(repositoryId != entity.RepositoryId)
                throw new StoragrError($"Mismatch between {nameof(repositoryId)} and {nameof(entity.RepositoryId)}!");
            
            if (await Exists(entity.RepositoryId, entity.Id)) 
                throw new ObjectAlreadyExistsError(null); // TODO

            if (!await _store.Verify(entity.RepositoryId, entity.Id, entity.Size))
                throw new StoragrError("Unable to verify with store.");

            await _backend.Insert(entity);
        }

        public async Task Delete(string repositoryId, string objectId)
        {
            if (!await Exists(repositoryId, objectId))
                throw new ObjectNotFoundError();
            
            await _backend.Delete(new ObjectEntity()
            {
                Id = objectId,
                RepositoryId = repositoryId
            });
            await _store.Delete(repositoryId, objectId);
        }

        public async Task DeleteAll(string repositoryId)
        {
            await _backend.Delete(
                await GetAll(repositoryId)
            );
            await _store.Delete(repositoryId);
        }

        public async Task<StoragrAction> NewVerifyAction(string repositoryId, string objectId)
        {
            if (!await Exists(repositoryId, objectId))
                throw new ObjectNotFoundError();

            var token = await _userService.GetAuthenticatedToken();
            return new StoragrAction
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 0,
                Href = $"{repositoryId}/objects/{objectId}"
            };
        }

        public async Task<StoragrAction> NewUploadAction(string repositoryId, string objectId)
        {
            if (await Exists(repositoryId, objectId))
                throw new ObjectAlreadyExistsError(null); // TODO

            return await _store.NewUploadAction(repositoryId, objectId);
        }

        public async Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId)
        {
            if (!await Exists(repositoryId, objectId))
                throw new ObjectNotFoundError();

            return await _store.NewDownloadAction(repositoryId, objectId);
        }
    }
}