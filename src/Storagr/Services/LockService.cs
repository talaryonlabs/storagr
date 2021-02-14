using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Services
{
    public class LockService : ILockService
    {
        private readonly IDistributedCache _cache;
        private readonly IBackendAdapter _backend;
        private readonly IUserService _userService;

        private readonly DistributedCacheEntryOptions _cacheEntryOptions;
        private readonly string _cacheKey;
        private readonly string _cacheCountKey;
        
        public LockService(IBackendAdapter backend, IDistributedCache cache, IUserService userService)
        {
            _backend = backend;
            _cache = cache;
            _userService = userService;

            _cacheKey = "STORAGR:LOCK";
            _cacheCountKey = $"{_cacheKey}:COUNT";
            
            _cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // TODO from config
        }

        public async Task<int> Count(string repositoryId)
        {
            if (await _cache.ExistsAsync(_cacheCountKey))
            {
                return await _cache.GetAsyncObject<int>(_cacheCountKey);
            }

            var count = await _backend.Count<LockEntity>(filter =>
            {
                filter.Equal(nameof(LockEntity.RepositoryId), repositoryId);
            });
            await _cache.SetAsyncObject(_cacheCountKey, count);

            return count;
        }

        public Task<bool> Exists(string repositoryId, string objectId) =>
            _backend.Exists<LockEntity>(filter => filter
                .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                .And()
                .Equal(nameof(LockEntity.Id), objectId)
            );
        
        public Task<bool> ExistsByPath(string repositoryId, string path) =>
            _backend.Exists<LockEntity>(filter => filter
                .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                .And()
                .Equal(nameof(LockEntity.Path), path)
            );

        public async Task<LockEntity> Get(string repositoryId, string lockId)
        {
            var lockEntity = await _backend.Get<LockEntity>(q =>
            {
                q.Where(f => f
                    .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                    .And()
                    .Equal(nameof(LockEntity.Id), lockId)
                );
            }) ?? throw new LockNotFoundError();
            
            lockEntity.Owner = await _backend.Get<UserEntity>(lockEntity.OwnerId);

            return lockEntity;
        }
        public async Task<LockEntity> GetByPath(string repositoryId, string path)
        {
            var lockEntity = await _backend.Get<LockEntity>(q =>
            {
                q.Where(f => f
                    .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                    .And()
                    .Equal(nameof(LockEntity.Path), path.Trim())
                );
            }) ?? throw new LockNotFoundError();
            
            lockEntity.Owner = await _backend.Get<UserEntity>(lockEntity.OwnerId);

            return lockEntity;
        }

        public async Task<IEnumerable<LockEntity>> GetMany(string repositoryId, IEnumerable<string> lockIds)
        {
            var users = await _userService.GetAll();
            return (await _backend.GetAll<LockEntity>(query =>
                    query.Where(filter => filter
                        .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                        .And()
                        .In(nameof(LockEntity.Id), lockIds)
                    )))
                .ToList()
                .Select(v =>
                {
                    v.Owner = users.FirstOrDefault(x => x.Id == v.OwnerId);
                    return v;
                });
        }

        public async Task<IEnumerable<LockEntity>> GetAll(string repositoryId)
        {
            var users = await _userService.GetAll();
            return (await _backend.GetAll<LockEntity>(query =>
                    query.Where(filter => filter
                        .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                    )))
                .ToList()
                .Select(v =>
                {
                    v.Owner = users.FirstOrDefault(x => x.Id == v.OwnerId);
                    return v;
                });
        }

        public async Task<IEnumerable<LockEntity>> GetAll(string repositoryId, LockSearchArgs searchArgs)
        {
            var users = await _userService.GetAll();
            return (await _backend.GetAll<LockEntity>(query =>
                    query.Where(filter =>
                    {
                        filter.Equal(nameof(LockEntity.RepositoryId), repositoryId);
                        
                        if (!string.IsNullOrEmpty(searchArgs.LockId))
                            filter.And().Like(nameof(LockEntity.Id), $"%{searchArgs.LockId}%");

                        if (!string.IsNullOrEmpty(searchArgs.Path))
                            filter.And().Like(nameof(LockEntity.Path), $"%{searchArgs.Path}%");
                    })))
                .ToList()
                .Select(v =>
                {
                    v.Owner = users.FirstOrDefault(x => x.Id == v.OwnerId);
                    return v;
                });
        }

        public async Task Create(string repositoryId, LockEntity entity)
        {
            if(repositoryId != entity.RepositoryId)
                throw new StoragrError($"Mismatch between {nameof(repositoryId)} and {nameof(entity.RepositoryId)}!");
            
            if (await Exists(entity.RepositoryId, entity.Id) || await ExistsByPath(entity.RepositoryId, entity.Path)) 
                throw new LockAlreadyExistsError(null); // TODO

            await _backend.Insert(entity);
        }

        public async Task Delete(string repositoryId, string lockId)
        {
            if(!await Exists(repositoryId, lockId))
                throw new LockNotFoundError();

            await _cache.RemoveAsync(_cacheCountKey);
            await _backend.Delete(new LockEntity()
            {
                Id = lockId,
                RepositoryId = repositoryId
            });
        }

        public async Task DeleteAll(string repositoryId)
        {
            await _cache.RemoveAsync(_cacheCountKey);
            await _backend.Delete(
                await GetAll(repositoryId)
            );
        }
    }
}