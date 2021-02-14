using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Server.Data.Entities;
using Storagr.Shared;

namespace Storagr.Server.Services
{
    public partial class StoragrService
    {
        private static class LockHelper
        {
            public static string NormalizePath(string path)
            {
                return path; // TODO
            }
        }
        
        private class LockItem :
            IStoragrServiceLock,
            IStoragrRunner<bool>
        {
            private readonly StoragrService _storagrService;
            private readonly string _repositoryIdOrName;
            private readonly string _lockIdOrPath;

            private LockEntity _createRequest;
            private bool _deleteRequest;
            private bool _forceRequest;

            public LockItem(StoragrService storagrService, string repositoryIdOrName, string lockIdOrPath)
            {
                _storagrService = storagrService;
                _repositoryIdOrName = repositoryIdOrName;
                _lockIdOrPath = lockIdOrPath;
            }

            LockEntity IStoragrRunner<LockEntity>.Run() => (this as IStoragrRunner<LockEntity>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<LockEntity> IStoragrRunner<LockEntity>.RunAsync(CancellationToken cancellationToken)
            {
                var cachedLock = await _storagrService
                    .Cache
                    .Key<LockEntity>(
                        StoragrCaching.GetLockKey(_repositoryIdOrName, _lockIdOrPath)
                    )
                    .RunAsync(cancellationToken);

                var lockEntity = cachedLock ?? await _storagrService
                    .Database
                    .First<LockEntity>()
                    .Join<RepositoryEntity>(nameof(LockEntity.RepositoryId), nameof(RepositoryEntity.Id))
                    .Join<UserEntity>(nameof(LockEntity.OwnerId), nameof(UserEntity.Id))
                    .With(select => select
                        .Column<RepositoryEntity>(nameof(RepositoryEntity.Name), nameof(LockEntity.RepositoryName))
                        .Column<UserEntity>(nameof(UserEntity.Username), nameof(LockEntity.OwnerName))
                    )
                    .Where(filter => filter
                        .Clamp(repoFilter => repoFilter
                            .Is<RepositoryEntity>(nameof(RepositoryEntity.Id))
                            .EqualTo(_repositoryIdOrName)
                            .Or()
                            .Is<RepositoryEntity>(nameof(RepositoryEntity.Name))
                            .EqualTo(_repositoryIdOrName)
                        )
                        .And()
                        .Clamp(lockFilter => lockFilter
                            .Is(nameof(LockEntity.Id))
                            .EqualTo(_lockIdOrPath)
                            .Or()
                            .Is(nameof(LockEntity.Path))
                            .EqualTo(LockHelper.NormalizePath(_lockIdOrPath))
                        )
                    )
                    .RunAsync(cancellationToken);

                if (_createRequest is not null)
                {
                    if (lockEntity is not null)
                        throw new LockAlreadyExistsError(lockEntity);

                    _createRequest.OwnerId = (
                        await (_storagrService as IStoragrService)
                            .Authorization()
                            .GetAuthenticatedUser()
                            .RunAsync(cancellationToken)
                    ).Id;
                    _createRequest.RepositoryId = (
                        await (_storagrService as IStoragrService)
                            .Repository(_repositoryIdOrName)
                            .RunAsync(cancellationToken)
                    ).Id;

                    lockEntity = await _storagrService
                        .Database
                        .Insert(_createRequest)
                        .RunAsync(cancellationToken);
                }

                if (lockEntity is null)
                    throw new LockNotFoundError();

                if (_deleteRequest)
                {
                    var user = await (_storagrService as IStoragrService)
                        .Authorization()
                        .GetAuthenticatedUser()
                        .RunAsync(cancellationToken);
                    if (!_forceRequest && !user.IsAdmin && user.Id != lockEntity.OwnerId)
                        throw new UnauthorizedError("Unable to delete lock - access denied.");

                    await Task.WhenAll(
                        _storagrService
                            .Cache
                            .RemoveMany(new[]
                            {
                                StoragrCaching.GetLockKey(lockEntity.RepositoryId, lockEntity.Id),
                                StoragrCaching.GetLockKey(lockEntity.RepositoryId, lockEntity.Path)
                            })
                            .RunAsync(cancellationToken),
                        _storagrService
                            .Database
                            .Delete(lockEntity)
                            .RunAsync(cancellationToken)
                    );
                }
                else if (cachedLock is null)
                {
                    await Task.WhenAll(
                        _storagrService
                            .Cache
                            .Key<LockEntity>(StoragrCaching.GetLockKey(lockEntity.RepositoryId, lockEntity.Id))
                            .Set(lockEntity)
                            .RunAsync(cancellationToken),
                        _storagrService
                            .Cache
                            .Key<LockEntity>(StoragrCaching.GetLockKey(lockEntity.RepositoryId, lockEntity.Path))
                            .Set(lockEntity)
                            .RunAsync(cancellationToken)
                    );
                }

                return lockEntity;
            }

            IStoragrRunner<bool> IStoragrExistable.Exists() => this;
            bool IStoragrRunner<bool>.Run() => (this as IStoragrRunner<bool>)
                .RunAsync()
                .RunSynchronouslyWithResult();
            
            async Task<bool> IStoragrRunner<bool>.RunAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await (this as IStoragrRunner<LockEntity>).RunAsync(cancellationToken);
                }
                catch (LockNotFoundError)
                {
                    return false;
                }

                return true;
            }

            IStoragrRunner<LockEntity> IStoragrCreatable<LockEntity>.Create()
            {
                _createRequest = new LockEntity()
                {
                    Id = StoragrHelper.UUID(),
                    Path = LockHelper.NormalizePath(_lockIdOrPath),
                    RepositoryId = null,
                    LockedAt = DateTime.Now,
                    OwnerId = null
                };
                return this;
            }

            IStoragrRunner<LockEntity> IStoragrDeletable<LockEntity>.Delete(bool force)
            {
                _deleteRequest = true;
                _forceRequest = force;
                return this;
            }
        }

        private class LockList :
            IStoragrServiceLocks,
            ILockParams
        {
            private readonly StoragrService _storagrService;
            private readonly string _repositoryIdOrName;
            private readonly LockEntity _entity;

            private int _take, _skip;
            private string _skipUntil;

            public LockList(StoragrService storagrService, string repositoryIdOrName)
            {
                _storagrService = storagrService;
                _repositoryIdOrName = repositoryIdOrName;
                _entity = new LockEntity();
            }

            IEnumerable<LockEntity> IStoragrRunner<IEnumerable<LockEntity>>.Run() =>
                (this as IStoragrRunner<IEnumerable<LockEntity>>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<IEnumerable<LockEntity>> IStoragrRunner<IEnumerable<LockEntity>>.RunAsync(CancellationToken cancellationToken)
            {
                var users = await (_storagrService as IStoragrService)
                    .Users()
                    .RunAsync(cancellationToken);
                
                var locks = await _storagrService
                    .Database
                    .Many<LockEntity>()
                    .Join<RepositoryEntity>(nameof(LockEntity.RepositoryId), nameof(RepositoryEntity.Id))
                    .Join<UserEntity>(nameof(LockEntity.OwnerId), nameof(UserEntity.Id))
                    .With(select => select
                        .Column<RepositoryEntity>(nameof(RepositoryEntity.Name), nameof(LockEntity.RepositoryName))
                        .Column<UserEntity>(nameof(UserEntity.Username), nameof(LockEntity.OwnerName))
                    )
                    .Where(filter => filter
                        .Clamp(repoFilter => repoFilter
                            .Is<RepositoryEntity>(nameof(RepositoryEntity.Id))
                            .EqualTo(_repositoryIdOrName)
                            .Or()
                            .Is<RepositoryEntity>(nameof(RepositoryEntity.Name))
                            .EqualTo(_repositoryIdOrName)
                        )
                        .And()
                        .Clamp(pattern =>
                        {
                            if (_entity.Id is not null)
                                pattern
                                    .Is(nameof(LockEntity.Id))
                                    .Like(_entity.Id)
                                    .Or();

                            if (_entity.Path is not null)
                                pattern
                                    .Is(nameof(LockEntity.Path))
                                    .Like(_entity.Path)
                                    .Or();

                            if (_entity.OwnerId is not null)
                            {
                                pattern
                                    .Is<UserEntity>(nameof(UserEntity.Id))
                                    .EqualTo(_entity.OwnerId)
                                    .Or()
                                    .Is<UserEntity>(nameof(UserEntity.Username))
                                    .EqualTo(_entity.OwnerId);
                            }
                        })
                    )
                    .RunAsync(cancellationToken);

                return locks
                    .Skip(_skip)
                    .SkipWhile(e => _skipUntil is not null && e.Id != _skipUntil)
                    .Take(_take)
                    .Select(v => (LockEntity) v);
            }
            
            IStoragrRunner<int> IStoragrCountable.Count()
            {
                return _storagrService
                    .Database
                    .Count<LockEntity>();
            }

            IStoragrEnumerable<LockEntity, ILockParams> IStoragrEnumerable<LockEntity, ILockParams>.Take(int count)
            {
                _take = count;
                return this;
            }

            IStoragrEnumerable<LockEntity, ILockParams> IStoragrEnumerable<LockEntity, ILockParams>.Skip(int count)
            {
                _skip = count;
                return this;
            }

            IStoragrEnumerable<LockEntity, ILockParams> IStoragrEnumerable<LockEntity, ILockParams>.SkipUntil(string cursor)
            {
                _skipUntil = cursor;
                return this;
            }

            IStoragrEnumerable<LockEntity, ILockParams> IStoragrEnumerable<LockEntity, ILockParams>.Where(Action<ILockParams> whereParams)
            {
                whereParams(this);
                return this;
            }

            ILockParams ILockParams.Id(string lockId)
            {
                _entity.Id = lockId;
                return this;
            }

            ILockParams ILockParams.Path(string lockedPath)
            {
                _entity.Path = lockedPath;
                return this;
            }
            
            ILockParams ILockParams.Owner(string owner)
            {
                _entity.OwnerId = owner;
                return this;
            }
        }
    }
}