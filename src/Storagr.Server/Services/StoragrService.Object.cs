using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Server.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server.Services
{
    public partial class StoragrService
    {
        private class ObjectItem :
            IStoragrServiceObject,
            IObjectParams,
            IStoragrRunner<bool>,
            IStoragrParams<ObjectEntity, IObjectParams>
        {
            private readonly StoragrService _storagrService;
            private readonly string _repositoryIdOrName;
            private readonly string _objectId;

            private ObjectEntity _createRequest;
            private bool _deleteRequest;

            public ObjectItem(StoragrService storagrService, string repositoryIdOrName, string objectId)
            {
                _storagrService = storagrService;
                _repositoryIdOrName = repositoryIdOrName;
                _objectId = objectId;
            }
        
            ObjectEntity IStoragrRunner<ObjectEntity>.Run() => (this as IStoragrRunner<ObjectEntity>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<ObjectEntity> IStoragrRunner<ObjectEntity>.RunAsync(CancellationToken cancellationToken)
            {
                var cachedObject = await _storagrService
                    .Cache
                    .Key<ObjectEntity>(StoragrCaching.GetObjectKey(_repositoryIdOrName, _objectId))
                    .RunAsync(cancellationToken);
                
                var objectEntity = cachedObject ?? await _storagrService
                    .Database
                    .First<ObjectEntity>()
                    .Join<RepositoryEntity>(nameof(ObjectEntity.RepositoryId), nameof(RepositoryEntity.Id))
                    .Where(filter => filter
                        .Clamp(repoFilter => repoFilter
                            .Is<RepositoryEntity>(nameof(RepositoryEntity.Id))
                            .EqualTo(_repositoryIdOrName)
                            .Or()
                            .Is<RepositoryEntity>(nameof(RepositoryEntity.Name))
                            .EqualTo(_repositoryIdOrName)
                        )
                        .And()
                        .Is(nameof(ObjectEntity.Id))
                        .EqualTo(_objectId)
                    )
                    .RunAsync(cancellationToken);

                if (_createRequest is not null)
                {
                    if (objectEntity is not null)
                        throw new ObjectAlreadyExistsError(objectEntity);

                    _createRequest.RepositoryId = (
                        await (_storagrService as IStoragrService)
                            .Repository(_repositoryIdOrName)
                            .RunAsync(cancellationToken)
                    ).Id;

                    objectEntity = await _storagrService
                        .Database
                        .Insert(_createRequest)
                        .RunAsync(cancellationToken);
                }

                if (objectEntity is null)
                    throw new ObjectNotFoundError();

                var cacheEntry = _storagrService
                    .Cache
                    .Key(StoragrCaching.GetObjectKey(objectEntity.RepositoryId, objectEntity.Id));

                if (_deleteRequest)
                {
                    await Task.WhenAll(
                        cacheEntry
                            .Delete()
                            .RunAsync(cancellationToken),
                        _storagrService
                            .Database
                            .Delete(objectEntity)
                            .RunAsync(cancellationToken)
                    );
                }
                else if (cachedObject is null)
                {
                    await cacheEntry
                        .Set(objectEntity)
                        .RunAsync(cancellationToken);
                }

                return objectEntity;
            }

            IStoragrRunner<bool> IStoragrExistable.Exists() => this;
            bool IStoragrRunner<bool>.Run() => (this as IStoragrRunner<bool>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<bool> IStoragrRunner<bool>.RunAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await (this as IStoragrRunner<ObjectEntity>).RunAsync(cancellationToken);
                }
                catch (ObjectNotFoundError)
                {
                    return false;
                }

                return true;
            }

            IStoragrParams<ObjectEntity, IObjectParams> IStoragrCreatable<ObjectEntity, IObjectParams>.Create()
            {
                _createRequest = new ObjectEntity()
                {
                    Id = _objectId,
                    RepositoryId = _repositoryIdOrName
                };
                return this;
            }
            
            IStoragrRunner<ObjectEntity> IStoragrDeletable<ObjectEntity>.Delete(bool force)
            {
                _deleteRequest = true;
                return this;
            }

            IStoragrRunner<ObjectEntity> IStoragrParams<ObjectEntity, IObjectParams>.With(Action<IObjectParams> withParams)
            {
                withParams(this);
                return this;
            }

            IObjectParams IObjectParams.Id(string objectId)
            {
                // skipping
                return this;
            }

            IObjectParams IObjectParams.RepositoryId(string repositoryIdOrName)
            {
                // skipping
                return this;
            }

            IObjectParams IObjectParams.Size(long sizeLimit)
            {
                _createRequest.Size = sizeLimit;
                return this;
            }
        }
        
        private class ObjectList :
            IStoragrServiceObjects,
            IObjectParams
        {
            private readonly StoragrService _storagrService;
            private readonly string _repositoryIdOrName;
            private readonly ObjectEntity _entity;
            
            private int _take, _skip;
            private string _skipUntil;

            public ObjectList(StoragrService storagrService, string repositoryIdOrName)
            {
                _storagrService = storagrService;
                _repositoryIdOrName = repositoryIdOrName;
                _entity = new ObjectEntity();
            }

            IEnumerable<ObjectEntity> IStoragrRunner<IEnumerable<ObjectEntity>>.Run() =>
                (this as IStoragrRunner<IEnumerable<ObjectEntity>>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<IEnumerable<ObjectEntity>> IStoragrRunner<IEnumerable<ObjectEntity>>.RunAsync(CancellationToken cancellationToken)
            {
                var objects = await _storagrService
                        .Database
                        .Many<ObjectEntity>()
                        .Join<RepositoryEntity>(nameof(ObjectEntity.RepositoryId), nameof(RepositoryEntity.Id))
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
                                        .Is(nameof(ObjectEntity.Id))
                                        .Like(_entity.Id);
                            })
                        )
                        .RunAsync(cancellationToken);

                return objects
                    .Skip(_skip)
                    .SkipWhile(e => _skipUntil is not null && e.Id != _skipUntil)
                    .Take(_take);
            }

            IStoragrRunner<int> IStoragrCountable.Count() => _storagrService
                .Database
                .Count<ObjectEntity>()
                .Join<RepositoryEntity>(nameof(ObjectEntity.RepositoryId), nameof(RepositoryEntity.Id))
                .Where(filter => filter
                    .Is<RepositoryEntity>(nameof(RepositoryEntity.Id))
                    .EqualTo(_repositoryIdOrName)
                    .Or()
                    .Is<RepositoryEntity>(nameof(RepositoryEntity.Name))
                    .EqualTo(_repositoryIdOrName)
                );

            IStoragrEnumerable<ObjectEntity, IObjectParams> IStoragrEnumerable<ObjectEntity, IObjectParams>.Take(int count)
            {
                _take = count;
                return this;
            }

            IStoragrEnumerable<ObjectEntity, IObjectParams> IStoragrEnumerable<ObjectEntity, IObjectParams>.Skip(int count)
            {
                _skip = count;
                return this;
            }

            IStoragrEnumerable<ObjectEntity, IObjectParams> IStoragrEnumerable<ObjectEntity, IObjectParams>.SkipUntil(string cursor)
            {
                _skipUntil = cursor;
                return this;
            }

            IStoragrEnumerable<ObjectEntity, IObjectParams> IStoragrEnumerable<ObjectEntity, IObjectParams>.Where(Action<IObjectParams> whereParams)
            {
                whereParams(this);
                return this;
            }

            IObjectParams IObjectParams.Id(string objectId)
            {
                _entity.Id = objectId;
                return this;
            }

            IObjectParams IObjectParams.RepositoryId(string repositoryId)
            {
                _entity.RepositoryId = repositoryId;
                return this;
            }

            IObjectParams IObjectParams.Size(long size)
            {
                _entity.Size = size;
                return this;
            }
        }
    }
}