using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Server.Data.Entities;
using Storagr.Server.Shared;

namespace Storagr.Server.Services
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

        public async Task<int> Count(string repositoryIdOrName, string ownerId, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetRepositoryCountKey();
            var useCache = (repositoryIdOrName is null && ownerId is null);
            if (useCache && await _cache.ExistsAsync(key, cancellationToken))
            {
                return await _cache.GetObjectAsync<int>(key, cancellationToken);
            }
            
            var count = await _database.Count<RepositoryEntity>(filter =>
            {
                if (repositoryIdOrName is not null)
                {
                    filter
                        .Like(nameof(RepositoryEntity.Id), repositoryIdOrName)
                        .Or()
                        .Like(nameof(RepositoryEntity.Name), repositoryIdOrName);
                }
                else if (ownerId is not null)
                    filter.Like(nameof(RepositoryEntity.OwnerId), ownerId);
            }, cancellationToken);

            if (useCache)
            {
                await _cache.SetObjectAsync(key, count, _cacheEntryOptions, cancellationToken);
            }

            return count;
        }


        public async Task<bool> Exists(string repositoryIdOrName, CancellationToken cancellationToken) =>
            await _cache.ExistsAsync(StoragrCaching.GetRepositoryKey(repositoryIdOrName), cancellationToken)
            ||
            await _database.Exists<RepositoryEntity>(filter =>
            {
                filter
                    .Equal(nameof(RepositoryEntity.Id), repositoryIdOrName)
                    .Or()
                    .Equal(nameof(RepositoryEntity.Name), repositoryIdOrName);
            }, cancellationToken);

        public async Task<RepositoryEntity> Get(string repositoryIdOrName, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetRepositoryKey(repositoryIdOrName);
            var repository = 
                await _cache.GetObjectAsync<RepositoryEntity>(key, cancellationToken) ??
                await _database.Get<RepositoryEntity>(filter =>
                {
                    filter
                        .Equal(nameof(RepositoryEntity.Id), repositoryIdOrName)
                        .Or()
                        .Equal(nameof(RepositoryEntity.Name), repositoryIdOrName);
                }, cancellationToken) ??
                throw new RepositoryNotFoundError();

            await _cache.SetObjectAsync(key, repository, _cacheEntryOptions, cancellationToken);

            return repository;
        }

        public Task<IEnumerable<RepositoryEntity>> GetMany(string repositoryIdOrName, string ownerId, CancellationToken cancellationToken) =>
            _database.GetMany<RepositoryEntity>(query => query.Where(filter =>
            {
                if (repositoryIdOrName is not null)
                {
                    filter
                        .Like(nameof(RepositoryEntity.Id), repositoryIdOrName)
                        .Or()
                        .Like(nameof(RepositoryEntity.Name), repositoryIdOrName);
                }
                else if (ownerId is not null)
                    filter.Like(nameof(RepositoryEntity.OwnerId), ownerId);
            }), cancellationToken);

        public Task<IEnumerable<RepositoryEntity>> GetAll(CancellationToken cancellationToken) =>
            _database.GetAll<RepositoryEntity>(cancellationToken);

        public async Task<RepositoryEntity> Create(RepositoryEntity newRepository, CancellationToken cancellationToken)
        {
            if (await Exists(newRepository.Name, cancellationToken))
                throw new RepositoryAlreadyExistsError(
                    await Get(newRepository.Name, cancellationToken)
                );

            var user = newRepository.OwnerId is not null
                ? await _userService.Get(newRepository.OwnerId, cancellationToken)
                : await _userService.GetAuthenticatedUser(cancellationToken);

            newRepository.Id = StoragrHelper.UUID();
            newRepository.OwnerId = user.Id;

            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetRepositoryCountKey(), cancellationToken),
                _database.Insert(newRepository, cancellationToken)
            );

            return newRepository;
        }

        public async Task<RepositoryEntity> Update(RepositoryEntity updatedRepository, CancellationToken cancellationToken = default)
        {
            var repository = await Get(updatedRepository.Id, cancellationToken);
            if (repository.Name != updatedRepository.Name && await Exists(updatedRepository.Name, cancellationToken))
                throw new RepositoryAlreadyExistsError(
                    await Get(updatedRepository.Name, cancellationToken)
                );

            if (repository.OwnerId != updatedRepository.OwnerId)
            {
                var user = updatedRepository.OwnerId is not null
                    ? await _userService.Get(updatedRepository.OwnerId, cancellationToken)
                    : await _userService.GetAuthenticatedUser(cancellationToken);
                
                updatedRepository.OwnerId = user.Id;
            }
            
            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetRepositoryCountKey(), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetRepositoryKey(repository.Id), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetRepositoryKey(repository.Name), cancellationToken),
                _database.Update(updatedRepository, cancellationToken)
            );

            return updatedRepository;
        }

        public async Task<RepositoryEntity> Delete(string repositoryIdOrName, CancellationToken cancellationToken)
        {
            var repository = await Get(repositoryIdOrName, cancellationToken);
            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetRepositoryCountKey(), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetRepositoryKey(repository.Id), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetRepositoryKey(repository.Name), cancellationToken),

                _lockService.UnlockAll(repository.Id, cancellationToken),
                _objectService.DeleteAll(repository.Id, cancellationToken),

                _database.Delete(new RepositoryEntity() {Id = repository.Id}, cancellationToken)
            );

            return repository;
        }

        public async Task GrantAccess(string repositoryIdOrName, string userId, RepositoryAccessType accessType, CancellationToken cancellationToken)
        {
            var repository = await Get(repositoryIdOrName, cancellationToken);
            if (repository.OwnerId == userId) // why granting something? ... he owns the repo :-P
                return;

            await _database.Insert(new RepositoryAccessEntity()
            {
                RepositoryId = repositoryIdOrName,
                UserId = userId,
                AccessType = accessType
            }, cancellationToken);
        }

        public async Task RevokeAccess(string repositoryIdOrName, string userId, CancellationToken cancellationToken)
        {
            var repository = await Get(repositoryIdOrName, cancellationToken);
            
            await _database.Delete(new RepositoryAccessEntity()
            {
                RepositoryId = repository.Id,
                UserId = userId
            }, cancellationToken);
        }

        public async Task<bool> HasAccess(string repositoryIdOrName, string userId, RepositoryAccessType accessType, CancellationToken cancellationToken)
        {
            var repository = await Get(repositoryIdOrName, cancellationToken);

            return repository.OwnerId == userId || (await _database.Get<RepositoryAccessEntity>(query =>
            {
                query.Where(filter => filter
                    .Equal(nameof(RepositoryAccessEntity.RepositoryId), repository.Id)
                    .And()
                    .Equal(nameof(RepositoryAccessEntity.UserId), userId)
                    .And()
                    .Equal(nameof(RepositoryAccessEntity.AccessType), accessType.ToString())
                );
            }, cancellationToken) is not null);
        }
    }
}