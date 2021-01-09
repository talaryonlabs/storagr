using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Services
{
    public class ObjectService : IObjectService
    {
        private readonly IDatabaseAdapter _database;
        private readonly IStoreAdapter _store;
        private readonly IUserService _userService;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _cacheEntryOptions;

        public ObjectService(IDatabaseAdapter database, IStoreAdapter store, IUserService userService, IDistributedCache cache)
        {
            _database = database;
            _store = store;
            _userService = userService;
            _cache = cache;
            _cacheEntryOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // TODO time from a global config
        }

        public async Task<int> Count(string repositoryId, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetObjectCountKey(repositoryId);
            if (await _cache.ExistsAsync(key, cancellationToken))
                return await _cache.GetObjectAsync<int>(key, cancellationToken);

            var count = await _database.Count<ObjectEntity>(filter =>
            {
                filter
                    .Equal(nameof(ObjectEntity.RepositoryId), repositoryId);
            }, cancellationToken);

            await _cache.SetObjectAsync(key, count, _cacheEntryOptions, cancellationToken);
            return count;
        }
            


        public async Task<bool> Exists(string repositoryId, string objectId, CancellationToken cancellationToken) =>
            await _cache.ExistsAsync(StoragrCaching.GetObjectKey(repositoryId, objectId), cancellationToken)
            ||
            await _database.Exists<ObjectEntity>(filter =>
                {
                    filter
                        .Equal(nameof(ObjectEntity.RepositoryId), repositoryId)
                        .And()
                        .Equal(nameof(ObjectEntity.Id), objectId);
                },
                cancellationToken);


        public async Task<ObjectEntity> Get(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetObjectKey(repositoryId, objectId);
            var obj =
                await _cache.GetObjectAsync<ObjectEntity>(key, cancellationToken) ??
                await _database.Get<ObjectEntity>(filter =>
                {
                    filter
                        .Equal(nameof(ObjectEntity.RepositoryId), repositoryId)
                        .And()
                        .Equal(nameof(ObjectEntity.Id), objectId);
                }, cancellationToken) ??
                throw new ObjectNotFoundError();

            await _cache.SetObjectAsync(key, obj, _cacheEntryOptions, cancellationToken);
            return obj;
        }

        public Task<IEnumerable<ObjectEntity>> GetMany(string repositoryId, IEnumerable<string> objectIds, CancellationToken cancellationToken) =>
            _database.GetMany<ObjectEntity>(filter =>
            {
                filter
                    .Equal(nameof(ObjectEntity.RepositoryId), repositoryId)
                    .And()
                    .In(nameof(ObjectEntity.Id), objectIds);
            }, cancellationToken);
        
        public Task<IEnumerable<ObjectEntity>> GetAll(string repositoryId, CancellationToken cancellationToken) =>
            _database.GetMany<ObjectEntity>(filter =>
            {
                filter
                    .Equal(nameof(ObjectEntity.RepositoryId), repositoryId);
            }, cancellationToken);

        public async Task<ObjectEntity> Add(string repositoryId, ObjectEntity newObject, CancellationToken cancellationToken)
        {
            if (await Exists(repositoryId, newObject.Id, cancellationToken))
            {
                throw new ObjectAlreadyExistsError(
                    await Get(repositoryId, newObject.Id, cancellationToken)
                );
            }

            if (!await _store.Finalize(repositoryId, newObject.Id, newObject.Size, cancellationToken))
            {
                throw null; // TODO
            }
            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetObjectCountKey(repositoryId), cancellationToken),
                _database.Insert(newObject, cancellationToken)
            );
            
            return newObject;
        }

        public async Task<ObjectEntity> Delete(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            var deletingObject = await Get(repositoryId, objectId, cancellationToken);

            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetObjectCountKey(repositoryId), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetObjectKey(repositoryId, objectId), cancellationToken),
                _store.Delete(repositoryId, objectId, cancellationToken),
                _database.Delete(new ObjectEntity()
                {
                    Id = objectId,
                    RepositoryId = repositoryId
                }, cancellationToken)
            );
            
            return deletingObject;
        }

        public async Task<IEnumerable<ObjectEntity>> DeleteAll(string repositoryId, CancellationToken cancellationToken)
        {
            var list = (
                await GetAll(repositoryId, cancellationToken)
            ).ToList();

            await Task.WhenAll(
                list
                    .Select(v => _cache.RemoveAsync(StoragrCaching.GetObjectKey(repositoryId, v.Id), cancellationToken))
                    .Concat(new[]
                    {
                        _cache.RemoveAsync(StoragrCaching.GetObjectCountKey(repositoryId), cancellationToken),
                        _store.Delete(repositoryId, cancellationToken),
                        _database.Delete<ObjectEntity>(list, cancellationToken)
                    })
            );

            return list;
        }

        public async Task<StoragrAction> NewVerifyAction(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            if (!await Exists(repositoryId, objectId, cancellationToken))
                throw new ObjectNotFoundError();

            var token = await _userService.GetAuthenticatedToken(cancellationToken);
            return new StoragrAction
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 0,
                Href = $"{repositoryId}/objects/{objectId}"
            };
        }

        public async Task<StoragrAction> NewUploadAction(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            if (await Exists(repositoryId, objectId, cancellationToken))
            {
                throw new ObjectAlreadyExistsError(
                    await Get(repositoryId, objectId, cancellationToken)
                );
            }

            return await _store.NewUploadAction(repositoryId, objectId, cancellationToken);
        }

        public async Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            if (!await Exists(repositoryId, objectId, cancellationToken))
                throw new ObjectNotFoundError();

            return await _store.NewDownloadAction(repositoryId, objectId, cancellationToken);
        }
    }
}