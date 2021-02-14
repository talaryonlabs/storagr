using System;
using System.Collections;
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
        private class RepositoryItem :
            IStoragrServiceRepository,
            IRepositoryParams,
            IStoragrRunner<bool>,
            IStoragrParams<RepositoryEntity, IRepositoryParams>
        {
            private readonly StoragrService _storagrService;
            private readonly string _repositoryIdOrName;
            
            private RepositoryEntity _createRequest;
            private Dictionary<string, object> _updateRequest;
            private bool _deleteRequest;

            public RepositoryItem(StoragrService storagrService, string repositoryIdOrName)
            {
                _storagrService = storagrService;
                _repositoryIdOrName = repositoryIdOrName;
            }

            RepositoryEntity IStoragrRunner<RepositoryEntity>.Run() => (this as IStoragrRunner<RepositoryEntity>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<RepositoryEntity> IStoragrRunner<RepositoryEntity>.RunAsync(CancellationToken cancellationToken)
            {
                var cachedRepository = await _storagrService
                    .Cache
                    .Key<RepositoryEntity>(StoragrCaching.GetRepositoryKey(_repositoryIdOrName))
                    .RunAsync(cancellationToken);

                var repositoryEntity = cachedRepository ?? await _storagrService
                    .Database
                    .First<RepositoryEntity>()
                    .Join<UserEntity>(nameof(RepositoryEntity.OwnerId), nameof(UserEntity.Id))
                    .With(select => select
                        .Column<UserEntity>(nameof(UserEntity.Username), nameof(RepositoryEntity.OwnerName))
                    )
                    .Where(filter => filter
                        .Is(nameof(RepositoryEntity.Id))
                        .EqualTo(_repositoryIdOrName)
                        .Or()
                        .Is(nameof(RepositoryEntity.Name))
                        .EqualTo(_repositoryIdOrName)
                    )
                    .RunAsync(cancellationToken);

                if (_createRequest is not null)
                {
                    if (repositoryEntity is not null)
                        throw new RepositoryAlreadyExistsError(repositoryEntity);

                    _createRequest.OwnerId = (
                        _createRequest.OwnerId is not null
                            ? await _storagrService
                                .User(_createRequest.OwnerId)
                                .RunAsync(cancellationToken)
                            : await _storagrService
                                .Authorization()
                                .GetAuthenticatedUser()
                                .RunAsync(cancellationToken)
                    ).Id;

                    repositoryEntity = await _storagrService
                        .Database
                        .Insert(_createRequest)
                        .RunAsync(cancellationToken);
                }

                if (repositoryEntity is null)
                    throw new RepositoryNotFoundError();

                if (_updateRequest is not null)
                {
                    if (_updateRequest.ContainsKey("name"))
                        repositoryEntity.Name = (string) _updateRequest["name"];

                    if (_updateRequest.ContainsKey("size_limit"))
                        repositoryEntity.SizeLimit = (ulong) _updateRequest["size_limit"];

                    if (_updateRequest.ContainsKey("owner"))
                        repositoryEntity.OwnerId = (
                            await _storagrService
                                .User((string) _updateRequest["owner"])
                                .RunAsync(cancellationToken)
                        ).Id;

                    await Task.WhenAll(
                        _storagrService
                            .Cache
                            .RemoveMany(new[]
                            {
                                StoragrCaching.GetRepositoryKey(repositoryEntity.Id),
                                StoragrCaching.GetRepositoryKey(repositoryEntity.Name)
                            })
                            .RunAsync(cancellationToken),
                        _storagrService
                            .Database
                            .Update(repositoryEntity)
                            .RunAsync(cancellationToken)
                    );
                }

                if (_deleteRequest)
                {
                    await Task.WhenAll(
                        _storagrService
                            .Cache
                            .RemoveMany(new[]
                            {
                                StoragrCaching.GetRepositoryKey(repositoryEntity.Id),
                                StoragrCaching.GetRepositoryKey(repositoryEntity.Name)
                            })
                            .RunAsync(cancellationToken),
                        _storagrService
                            .Database
                            .Delete(repositoryEntity)
                            .RunAsync(cancellationToken)
                    );
                }
                else if (cachedRepository is null)
                {
                    await Task.WhenAll(
                        _storagrService
                            .Cache
                            .Key<RepositoryEntity>(StoragrCaching.GetRepositoryKey(repositoryEntity.Id))
                            .Set(repositoryEntity)
                            .RunAsync(cancellationToken),
                        _storagrService
                            .Cache
                            .Key<RepositoryEntity>(StoragrCaching.GetRepositoryKey(repositoryEntity.Name))
                            .Set(repositoryEntity)
                            .RunAsync(cancellationToken)
                    );
                }
                return repositoryEntity;
            }

            IStoragrParams<RepositoryEntity, IRepositoryParams> IStoragrCreatable<RepositoryEntity, IRepositoryParams>.Create()
            {
                _createRequest = new RepositoryEntity()
                {
                    Id = StoragrHelper.UUID()
                };
                return this;
            }

            IStoragrParams<RepositoryEntity, IRepositoryParams> IStoragrUpdatable<RepositoryEntity, IRepositoryParams>.Update()
            {
                _updateRequest = new Dictionary<string, object>();
                return this;
            }

            IStoragrRunner<RepositoryEntity> IStoragrDeletable<RepositoryEntity>.Delete(bool force)
            {
                _deleteRequest = true;
                return this;
            }
            
            IStoragrRunner<bool> IStoragrExistable.Exists() => this;

            bool IStoragrRunner<bool>.Run() => (this as IStoragrRunner<bool>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<bool> IStoragrRunner<bool>.RunAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await (this as IStoragrRunner<RepositoryEntity>).RunAsync(cancellationToken);
                }
                catch (RepositoryNotFoundError)
                {
                    return false;
                }

                return true;
            }

            IStoragrRunner<RepositoryEntity> IStoragrParams<RepositoryEntity, IRepositoryParams>.With(
                Action<IRepositoryParams> withParams)
            {
                withParams(this);
                return this;
            }
            
            IRepositoryParams IRepositoryParams.Id(string repositoryId)
            {
                // skipping - cannot modifiy id
                return this;
            }

            IRepositoryParams IRepositoryParams.Name(string name)
            {
                if (_createRequest is not null)
                    _createRequest.Name = name;

                _updateRequest?.Add("name", name);
                return this;
            }

            IRepositoryParams IRepositoryParams.Owner(string owner)
            {
                if (_createRequest is not null)
                    _createRequest.OwnerId = owner;

                _updateRequest?.Add("owner", owner);
                return this;
            }

            IRepositoryParams IRepositoryParams.SizeLimit(ulong sizeLimit)
            {
                if (_createRequest is not null)
                    _createRequest.SizeLimit = sizeLimit;

                _updateRequest?.Add("size_limit", sizeLimit);
                return this;
            }

            public IStoragrServiceLock Lock(string lockIdOrPath) =>
                new LockItem(_storagrService, _repositoryIdOrName, lockIdOrPath);

            public IStoragrServiceLocks Locks() => 
                new LockList(_storagrService, _repositoryIdOrName);

            public IStoragrServiceObject Object(string objectId) =>
                new ObjectItem(_storagrService, _repositoryIdOrName, objectId);

            public IStoragrServiceObjects Objects() =>
                new ObjectList(_storagrService, _repositoryIdOrName);

            
            IStoragrRunner IStoragrServiceRepository.GrantAccess(string userId, RepositoryAccessType accessType)
            {
                return new RepositoryPermissions(_storagrService, _repositoryIdOrName);
            }

            IStoragrRunner IStoragrServiceRepository.RevokeAccess(string userId)
            {
                throw new NotImplementedException();
            }

            IStoragrRunner<bool> IStoragrServiceRepository.HasAccess(string userId, RepositoryAccessType accessType)
            {
                throw new NotImplementedException();
            }
        }

        private class RepositoryList :
            IStoragrServiceRepositories,
            IRepositoryParams
        {
            private readonly StoragrService _storagrService;
            private readonly RepositoryEntity _entity;

            private int _take, _skip;
            private string _skipUntil;

            public RepositoryList(StoragrService storagrService)
            {
                _storagrService = storagrService;
                _entity = new RepositoryEntity();
            }

            IEnumerable<RepositoryEntity> IStoragrRunner<IEnumerable<RepositoryEntity>>.Run() =>
                (this as IStoragrRunner<IEnumerable<RepositoryEntity>>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<IEnumerable<RepositoryEntity>> IStoragrRunner<IEnumerable<RepositoryEntity>>.RunAsync(
                CancellationToken cancellationToken)
            {
                var repositories = await _storagrService
                    .Database
                    .Many<RepositoryEntity>()
                    .Join<UserEntity>(nameof(RepositoryEntity.OwnerId), nameof(UserEntity.Id))
                    .With(select => select
                        .Column<UserEntity>(nameof(UserEntity.Username), nameof(RepositoryEntity.OwnerName))
                    )
                    .Where(filter =>
                    {
                        if (_entity.Id is not null)
                        {
                            filter
                                .Is(nameof(RepositoryEntity.Id))
                                .Like(_entity.Id)
                                .Or();
                        }

                        if (_entity.Name is not null)
                        {
                            filter
                                .Is(nameof(RepositoryEntity.Name))
                                .Like(_entity.Name)
                                .Or();
                        }

                        if (_entity.OwnerId is not null)
                        {
                            filter
                                .Is<UserEntity>(nameof(UserEntity.Id))
                                .Like(_entity.OwnerId)
                                .Or()
                                .Is<UserEntity>(nameof(UserEntity.Username))
                                .Like(_entity.OwnerId)
                                .Or();
                        }
                    })
                    .RunAsync(cancellationToken);

                return repositories
                    .Skip(_skip)
                    .SkipWhile(e => _skipUntil is not null && e.Id != _skipUntil)
                    .Take(_take)
                    .Select(v => (RepositoryEntity) v);
            }

            IStoragrRunner<int> IStoragrCountable.Count() => _storagrService
                .Database
                .Count<RepositoryEntity>();

            IStoragrEnumerable<RepositoryEntity, IRepositoryParams>
                IStoragrEnumerable<RepositoryEntity, IRepositoryParams>.Take(int count)
            {
                _take = count;
                return this;
            }

            IStoragrEnumerable<RepositoryEntity, IRepositoryParams>
                IStoragrEnumerable<RepositoryEntity, IRepositoryParams>.Skip(int count)
            {
                _skip = count;
                return this;
            }

            IStoragrEnumerable<RepositoryEntity, IRepositoryParams>
                IStoragrEnumerable<RepositoryEntity, IRepositoryParams>.SkipUntil(string cursor)
            {
                _skipUntil = cursor;
                return this;
            }

            IStoragrEnumerable<RepositoryEntity, IRepositoryParams>
                IStoragrEnumerable<RepositoryEntity, IRepositoryParams>.Where(
                    Action<IRepositoryParams> whereParams)
            {
                whereParams(this);
                return this;
            }


            IRepositoryParams IRepositoryParams.Id(string repositoryId)
            {
                _entity.Id = repositoryId;
                return this;
            }

            IRepositoryParams IRepositoryParams.Name(string name)
            {
                _entity.Name = name;
                return this;
            }

            IRepositoryParams IRepositoryParams.Owner(string owner)
            {
                _entity.OwnerId = owner;
                return this;
            }

            IRepositoryParams IRepositoryParams.SizeLimit(ulong sizeLimit)
            {
                _entity.SizeLimit = sizeLimit;
                return this;
            }
        }

        private class RepositoryPermissions : 
            IStoragrRunner
        {
            private readonly StoragrService _storagrService;
            private readonly string _repositoryIdOrName;

            public RepositoryPermissions(StoragrService storagrService, string repositoryIdOrName)
            {
                _storagrService = storagrService;
                _repositoryIdOrName = repositoryIdOrName;
            }

            void IStoragrRunner.Run() => (this as IStoragrRunner)
                .RunAsync()
                .RunSynchronously();

            Task IStoragrRunner.RunAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}