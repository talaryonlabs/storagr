using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Server.Data.Entities;

namespace Storagr.Server.Services
{
    public class LockService : ILockService
    {
        private static string NormalizePath(string inputPath)
        {
            return inputPath; // TODO normalizing path (to correct  path if neccessary)
        }
        
        private readonly IDatabaseAdapter _database;
        private readonly IUserService _userService;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _cacheEntryOptions;

        public LockService(IDatabaseAdapter database, IUserService userService, IDistributedCache cache)
        {
            _database = database;
            _userService = userService;
            _cache = cache;
            _cacheEntryOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // TODO time from a global config
        }

        public async Task<int> Count(string repositoryId, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetLockCountKey(repositoryId);
            if(await _cache.ExistsAsync(key, cancellationToken))
                return await _cache.GetObjectAsync<int>(key, cancellationToken);
            
            var count = await _database.Count<LockEntity>(filter =>
            {
                filter
                    .Equal(nameof(LockEntity.RepositoryId), repositoryId);
            }, cancellationToken);

            await _cache.SetObjectAsync(key, count, _cacheEntryOptions, cancellationToken);
            return count;
        } 

        public async Task<bool> Exists(string repositoryId, string lockId, CancellationToken cancellationToken) =>
            await _cache.ExistsAsync(StoragrCaching.GetLockKey(repositoryId, lockId), cancellationToken)
            ||
            await _database.Exists<LockEntity>(filter =>
            {
                filter
                    .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                    .And()
                    .Equal(nameof(LockEntity.Id), lockId);
            }, cancellationToken);
        
        public async Task<bool> ExistsByPath(string repositoryId, string lockedPath, CancellationToken cancellationToken) =>
            await _cache.ExistsAsync(StoragrCaching.GetLockKey(repositoryId, lockedPath), cancellationToken)
            ||
            await _database.Exists<LockEntity>(filter =>
            {
                filter
                    .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                    .And()
                    .Equal(nameof(LockEntity.Path), NormalizePath(lockedPath));
            }, cancellationToken);

        public async Task<LockEntity> Get(string repositoryId, string lockId, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetLockKey(repositoryId, lockId);
            var lck =
                await _cache.GetObjectAsync<LockEntity>(key, cancellationToken) ??
                await _database.Get<LockEntity>(filter =>
                {
                    filter
                        .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                        .And()
                        .Equal(nameof(LockEntity.Id), lockId);
                }, cancellationToken) ??
                throw new LockNotFoundError();

            await _cache.SetObjectAsync(key, lck, _cacheEntryOptions, cancellationToken);
            lck.Owner = await _userService.Get(lck.OwnerId, cancellationToken);
            return lck;
        }
        public async Task<LockEntity> GetByPath(string repositoryId, string lockedPath, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetLockKey(repositoryId, lockedPath);
            var lck =
                await _cache.GetObjectAsync<LockEntity>(key, cancellationToken) ??
                await _database.Get<LockEntity>(filter =>
                {
                    filter
                        .Equal(nameof(LockEntity.RepositoryId), repositoryId)
                        .And()
                        .Equal(nameof(LockEntity.Path), lockedPath);
                }, cancellationToken) ??
                throw new LockNotFoundError();

            await _cache.SetObjectAsync(key, lck, _cacheEntryOptions, cancellationToken);
            lck.Owner = await _userService.Get(lck.OwnerId, cancellationToken);
            return lck;
        }

        public async Task<IEnumerable<LockEntity>> GetMany(string repositoryId, string lockId, string lockedPath, CancellationToken cancellationToken)
        {
            var users = await _userService.GetAll(cancellationToken);
            return (await _database.GetMany<LockEntity>(filter =>
                {
                    filter.Equal(nameof(LockEntity.RepositoryId), repositoryId);

                    if (lockId is not null)
                        filter.And().Like(nameof(LockEntity.Id), $"%{lockId}%");

                    if (lockedPath is not null)
                        filter.And().Like(nameof(LockEntity.Path), $"%{NormalizePath(lockedPath)}%");
                }, cancellationToken))
                .ToList()
                .Select(v =>
                {
                    v.Owner = users.FirstOrDefault(x => x.Id == v.OwnerId);
                    return v;
                });
        }

        public async Task<IEnumerable<LockEntity>> GetAll(string repositoryId, CancellationToken cancellationToken)
        {
            var users = await _userService.GetAll(cancellationToken);
            
            return (await _database.GetMany<LockEntity>(filter =>
                {
                    filter
                        .Equal(nameof(LockEntity.RepositoryId), repositoryId);
                }, cancellationToken))
                .ToList()
                .Select(v =>
                {
                    v.Owner = users.FirstOrDefault(x => x.Id == v.OwnerId);
                    return v;
                });
        }
        
        public async Task<LockEntity> Lock(string repositoryId, string path, CancellationToken cancellationToken)
        {
            if (await ExistsByPath(repositoryId, path, cancellationToken))
            {
                throw new LockAlreadyExistsError(
                    await GetByPath(repositoryId, path, cancellationToken)
                );
            }

            var user = await _userService.GetAuthenticatedUser(cancellationToken);
            var newLock = new LockEntity()
            {
                Id = StoragrHelper.UUID(),
                OwnerId = user.Id,
                LockedAt = DateTime.Now,
                Path = path,
                RepositoryId = repositoryId
            };

            await _database.Insert(newLock, cancellationToken);

            return newLock;
        }

        public async Task<LockEntity> Unlock(string repositoryId, string lockId, CancellationToken cancellationToken = default)
        {
            var existingLock = await Get(repositoryId, lockId, cancellationToken);

            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetLockCountKey(repositoryId), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetLockKey(repositoryId, existingLock.Id), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetLockKey(repositoryId, existingLock.Path), cancellationToken),
                _database.Delete(new LockEntity()
                {
                    Id = lockId,
                    RepositoryId = repositoryId
                }, cancellationToken)
            );

            return existingLock;
        }

        public async Task<IEnumerable<LockEntity>> UnlockAll(string repositoryId, CancellationToken cancellationToken = default)
        {
            var list = (
                await GetAll(repositoryId, cancellationToken)
            ).ToList();

            await Task.WhenAll(
                list
                    .SelectMany(v => new[]
                    {
                        _cache.RemoveAsync(StoragrCaching.GetLockKey(repositoryId, v.Id), cancellationToken),
                        _cache.RemoveAsync(StoragrCaching.GetLockKey(repositoryId, v.Path), cancellationToken)
                    })
                    .Concat(new[]
                    {
                        _cache.RemoveAsync(StoragrCaching.GetLockCountKey(repositoryId), cancellationToken),
                        _database.Delete<LockEntity>(list, cancellationToken)
                    })
            );

            return list;
        }
    }
}