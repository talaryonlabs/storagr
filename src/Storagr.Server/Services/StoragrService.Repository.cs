using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Server.Data.Entities;
using Storagr.Server.Shared;
using Storagr.Server.Shared.Data;

namespace Storagr.Server.Services
{
    public partial class StoragrService
    {
        private class StoragrRepositoryService :
            IRepositoryServiceItem,
            IStoragrParams<RepositoryEntity, IStoragrRepositoryParams>,
            IStoragrRepositoryParams
        {
            private readonly StoragrService _storagrService;
            private readonly string _repositoryIdOrName;

            private RepositoryEntity _repository;
            private bool _create, _update, _delete;

            public StoragrRepositoryService(StoragrService storagrService, string repositoryIdOrName)
            {
                _storagrService = storagrService;
                _repositoryIdOrName = repositoryIdOrName;
            }

            RepositoryEntity IStoragrRunner<RepositoryEntity>.Run() => (this as IStoragrRunner<RepositoryEntity>)
                .RunAsync()
                .RunSynchronouslyWithResult();


            async Task<RepositoryEntity> IStoragrRunner<RepositoryEntity>.RunAsync(CancellationToken cancellationToken)
            {
                var user = await (_storagrService as IUserService)
                    .User(_repository.OwnerId)
                    .RunAsync(cancellationToken);

                var repository = await (_storagrService as IRepositoryService)
                    .Repository(_repositoryIdOrName)
                    .Exists()
                    .RunAsync(cancellationToken);
                
                
                if (_createRequest is not null)
                {
                    _createRequest.Id = StoragrHelper.UUID();
                    
                    _storagrService.Database.Update(_createRequest, cancellationToken);
                }
                if (_repository is not null) _storagrService.Database.Update(_repository, cancellationToken);
            }

            IStoragrParams<RepositoryEntity, IStoragrRepositoryParams> IStoragrCreatable<RepositoryEntity, IStoragrRepositoryParams>.Create()
            {
                _create = true;
                _repository = new RepositoryEntity()
                {
                    Id = StoragrHelper.UUID()
                };
                return this;
            }

            IStoragrParams<RepositoryEntity, IStoragrRepositoryParams> IStoragrUpdatable<RepositoryEntity, IStoragrRepositoryParams>.Update()
            {
                _update = true;
                _repository = new RepositoryEntity();
                return this;
            }

            IStoragrRunner<RepositoryEntity> IStoragrDeletable<RepositoryEntity>.Delete(bool force)
            {
                _delete = true;
                return this;
            }

            IObjectServiceItem IObjectService.Object(string objectId)
            {
                throw new System.NotImplementedException();
            }

            IObjectServiceList IObjectService.Objects()
            {
                throw new System.NotImplementedException();
            }

            ILockServiceItem ILockService.Lock(string lockIdOrPath)
            {
                throw new System.NotImplementedException();
            }

            ILockServiceList ILockService.Locks()
            {
                throw new System.NotImplementedException();
            }

            IStoragrRunner<bool> IRepositoryServiceItem.Exists()
            {
                throw new System.NotImplementedException();
            }

            IStoragrRunner IRepositoryServiceItem.GrantAccess(string userId, RepositoryAccessType accessType)
            {
                throw new System.NotImplementedException();
            }

            IStoragrRunner IRepositoryServiceItem.RevokeAccess(string userId)
            {
                throw new System.NotImplementedException();
            }

            IStoragrRunner<bool> IRepositoryServiceItem.HasAccess(string userId, RepositoryAccessType accessType)
            {
                throw new System.NotImplementedException();
            }


            IStoragrRepositoryParams IStoragrRepositoryParams.Id(string repositoryId)
            {
                // skip - cannot change id
                return this;
            }

            IStoragrRepositoryParams IStoragrRepositoryParams.Name(string name)
            {
                _repository.Name = name;
                return this;
            }

            IStoragrRepositoryParams IStoragrRepositoryParams.Owner(string owner)
            {
                _repository.OwnerId = owner;
                return this;
            }

            IStoragrRepositoryParams IStoragrRepositoryParams.SizeLimit(ulong sizeLimit)
            {
                _repository.SizeLimit = sizeLimit;
                return this;
            }

            public IStoragrRunner<RepositoryEntity> With(Action<IStoragrRepositoryParams> withParams)
            {
                withParams(this);
                return this;
            }
        }

        private class StoragrRepositoryServiceList :
            IRepositoryServiceList,
            IStoragrRepositoryParams,
            IStoragrRunner<int>
        {
            private readonly StoragrService _storagrService;
            private readonly StoragrRepositoryListArgs _listArgs;

            public StoragrRepositoryServiceList(StoragrService storagrService)
            {
                _storagrService = storagrService;
                _listArgs = new StoragrRepositoryListArgs();
            }

            IStoragrRunner<int> IStoragrCountable.Count()
            {
                return this;
            }

            int IStoragrRunner<int>.Run() => (this as IStoragrRunner<int>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            Task<int> IStoragrRunner<int>.RunAsync(CancellationToken cancellationToken)
            {
                return _storagrService
                    .Database
                    .Count<RepositoryEntity>(cancellationToken);
            }

            IEnumerable<RepositoryEntity> IStoragrRunner<IEnumerable<RepositoryEntity>>.Run() =>
                (this as IStoragrRunner<IEnumerable<RepositoryEntity>>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            Task<IEnumerable<RepositoryEntity>> IStoragrRunner<IEnumerable<RepositoryEntity>>.RunAsync(
                CancellationToken cancellationToken)
            {
                return _storagrService
                    .Database
                    .GetMany<RepositoryEntity>(query => query
                            .Limit(_listArgs.Limit)
                            .Offset(_listArgs.Skip)
                            .Where(filter =>
                            {
                                if (_listArgs.Id is not null)
                                {
                                    filter.Like(nameof(RepositoryEntity.Id), $"%{_listArgs.Id}%").Or();
                                }

                                if (_listArgs.Name is not null)
                                {
                                    filter.Like(nameof(RepositoryEntity.Name), $"%{_listArgs.Name}%").Or();
                                }
                            }),
                        cancellationToken
                    );
            }



            IStoragrEnumerable<RepositoryEntity, IStoragrRepositoryParams>
                IStoragrEnumerable<RepositoryEntity, IStoragrRepositoryParams>.Take(int count)
            {
                _listArgs.Limit = count;
                return this;
            }

            IStoragrEnumerable<RepositoryEntity, IStoragrRepositoryParams>
                IStoragrEnumerable<RepositoryEntity, IStoragrRepositoryParams>.Skip(int count)
            {
                _listArgs.Skip = count;
                return this;
            }

            IStoragrEnumerable<RepositoryEntity, IStoragrRepositoryParams>
                IStoragrEnumerable<RepositoryEntity, IStoragrRepositoryParams>.SkipUntil(string cursor)
            {
                _listArgs.Cursor = cursor;
                return this;
            }

            IStoragrEnumerable<RepositoryEntity, IStoragrRepositoryParams>
                IStoragrEnumerable<RepositoryEntity, IStoragrRepositoryParams>.Where(Action<IStoragrRepositoryParams> whereParams)
            {
                whereParams(this);
                return this;
            }

            IStoragrRepositoryParams IStoragrRepositoryParams.Id(string repositoryId)
            {
                _listArgs.Id = repositoryId;
                return this;
            }

            IStoragrRepositoryParams IStoragrRepositoryParams.Name(string name)
            {
                _listArgs.Name = name;
                return this;
            }

            IStoragrRepositoryParams IStoragrRepositoryParams.Owner(string owner)
            {
                _listArgs.Owner = owner;
                return this;
            }

            IStoragrRepositoryParams IStoragrRepositoryParams.SizeLimit(ulong sizeLimit)
            {
                _listArgs.SizeLimit = sizeLimit;
                return this;
            }
        }
    }
}