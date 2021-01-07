using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Data.Entities;
using Storagr.Shared;

namespace Storagr.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly IDatabaseAdapter _database;
        private readonly ILockService _lockService;
        private readonly IObjectService _objectService;
        private readonly IUserService _userService;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _cacheEntryOptions;

        public RepositoryService(IDatabaseAdapter database, ILockService lockService, IObjectService objectService, IDistributedCache cache, IUserService userService)
        {
            _database = database;
            _lockService = lockService;
            _objectService = objectService;
            _cache = cache;
            _userService = userService;
            _cacheEntryOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // TODO time from a global config
        }

        public async Task<int> Count(string repositoryId, string ownerId, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetRepositoryCountKey();
            var useCache = (repositoryId is null && ownerId is null);
            if (useCache && await _cache.ExistsAsync(key, cancellationToken))
            {
                return await _cache.GetObjectAsync<int>(key, cancellationToken);
            }
            
            var count = await _database.Count<RepositoryEntity>(filter =>
            {
                if (repositoryId is not null)
                    filter.Like(nameof(RepositoryEntity.Id), repositoryId);
                else if (ownerId is not null)
                    filter.Like(nameof(RepositoryEntity.OwnerId), ownerId);
            }, cancellationToken);

            if (useCache)
            {
                await _cache.SetObjectAsync(key, count, _cacheEntryOptions, cancellationToken);
            }

            return count;
        }
            

        public async Task<bool> Exists(string repositoryId, CancellationToken cancellationToken) =>
            await _cache.ExistsAsync(StoragrCaching.GetRepositoryKey(repositoryId), cancellationToken) 
            ||
            await _database.Exists<RepositoryEntity>(repositoryId, cancellationToken);

        public async Task<RepositoryEntity> Get(string repositoryId, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetRepositoryKey(repositoryId);
            var repository = 
                await _cache.GetObjectAsync<RepositoryEntity>(key, cancellationToken) ??
                await _database.Get<RepositoryEntity>(repositoryId, cancellationToken) ?? 
                throw new RepositoryNotFoundError();

            await _cache.SetObjectAsync(key, repository, _cacheEntryOptions, cancellationToken);

            return repository;
        }

        public Task<IEnumerable<RepositoryEntity>> GetMany(string repositoryId, string ownerId, CancellationToken cancellationToken) =>
            _database.GetMany<RepositoryEntity>(query => query.Where(filter =>
            {
                if (repositoryId is not null)
                    filter.Like(nameof(RepositoryEntity.Id), repositoryId);
                else if (ownerId is not null)
                    filter.Like(nameof(RepositoryEntity.OwnerId), ownerId);
            }), cancellationToken);

        public Task<IEnumerable<RepositoryEntity>> GetAll(CancellationToken cancellationToken) =>
            _database.GetAll<RepositoryEntity>(cancellationToken);
        
        public async Task<RepositoryEntity> Create(RepositoryEntity newRepository, CancellationToken cancellationToken)
        {
            var user = await _userService.Get(newRepository.OwnerId, cancellationToken) ??
                       throw new UserNotFoundError();

            var repository = await _database.Get<RepositoryEntity>(newRepository.Id, cancellationToken);
            if(repository is not null)
                throw new RepositoryAlreadyExistsError(repository);

            newRepository.OwnerId = user.Id;
            
            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetRepositoryCountKey(), cancellationToken),
                _database.Insert(newRepository, cancellationToken)
            );

            return newRepository;
        }

        public async Task<RepositoryEntity> Delete(string repositoryId, CancellationToken cancellationToken)
        {
            var repository = await _database.Get<RepositoryEntity>(repositoryId, cancellationToken) ??
                             throw new RepositoryNotFoundError();

            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetRepositoryCountKey(), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetRepositoryKey(repositoryId), cancellationToken),

                _lockService.UnlockAll(repositoryId, cancellationToken),
                _objectService.DeleteAll(repositoryId, cancellationToken),

                _database.Delete(new RepositoryEntity() {Id = repositoryId}, cancellationToken)
            );

            return repository;
        }

        public async Task GrantAccess(string repositoryId, string userId, RepositoryAccessType accessType, CancellationToken cancellationToken)
        {
            var repository = await _database.Get<RepositoryEntity>(repositoryId, cancellationToken) ??
                             throw new RepositoryNotFoundError();

            if (repository.OwnerId == userId) // why granting something? ... he owns the repo :-P
                return;

            await _database.Insert(new RepositoryAccessEntity()
            {
                RepositoryId = repositoryId,
                UserId = userId,
                AccessType = accessType
            }, cancellationToken);
        }

        public async Task RevokeAccess(string repositoryId, string userId, CancellationToken cancellationToken)
        {
            if(!await Exists(repositoryId, cancellationToken))
                throw new RepositoryNotFoundError();

            await _database.Delete(new RepositoryAccessEntity()
            {
                RepositoryId = repositoryId,
                UserId = userId
            }, cancellationToken);
        }

        public async Task<bool> HasAccess(string repositoryId, string userId, RepositoryAccessType accessType, CancellationToken cancellationToken)
        {
            var repository = await _database.Get<RepositoryEntity>(repositoryId, cancellationToken) ??
                             throw new RepositoryNotFoundError();

            return repository.OwnerId == userId || (await _database.Get<RepositoryAccessEntity>(query =>
            {
                query.Where(filter => filter
                    .Equal(nameof(RepositoryAccessEntity.RepositoryId), repositoryId)
                    .And()
                    .Equal(nameof(RepositoryAccessEntity.UserId), userId)
                    .And()
                    .Equal(nameof(RepositoryAccessEntity.AccessType), accessType.ToString())
                );
            }, cancellationToken) is not null);
        }
    }
}