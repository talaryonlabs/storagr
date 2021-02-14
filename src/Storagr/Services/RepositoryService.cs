using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Storagr.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly IBackendAdapter _backendAdapter;

        public RepositoryService(IBackendAdapter backendAdapter)
        {
            _backendAdapter = backendAdapter;
        }

        public Task<int> Count() => 
            _backendAdapter.Count<RepositoryEntity>();

        public Task<bool> Exists(string repositoryId) =>
            _backendAdapter.Exists<RepositoryEntity>(repositoryId);

        public async Task<RepositoryEntity> Get(string repositoryId) =>
            await _backendAdapter.Get<RepositoryEntity>(repositoryId) ?? throw new RepositoryNotFoundError();

        public Task<IEnumerable<RepositoryEntity>> GetMany(IEnumerable<string> entityIds) =>
            _backendAdapter.GetAll<RepositoryEntity>(query =>
            {
                query.Where(filter => filter
                    .In(nameof(RepositoryEntity.Id), entityIds)
                );
            });

        public Task<IEnumerable<RepositoryEntity>> GetAll() =>
            _backendAdapter.GetAll<RepositoryEntity>();

        public Task<IEnumerable<RepositoryEntity>> GetAll(RepositorySearchArgs searchArgs) =>
            _backendAdapter.GetAll<RepositoryEntity>(q =>
            {
                q.Where(f => f.Like(nameof(RepositoryEntity.Id), searchArgs.RepositoryId));
            });

        public async Task Create(RepositoryEntity entity)
        {
            // TODO if OwnerId is null => owner will be current user
            var user = await _backendAdapter.Get<UserEntity>(query =>
                query.Where(filter => filter
                    .Equal(nameof(UserEntity.Id), entity.OwnerId)
                    .Or()
                    .Equal(nameof(UserEntity.Username), entity.OwnerId)
                )
            ); // TODO replace with UserService.GetByName

            if (user is null)
                throw new UserNotFoundError();

            if(await _backendAdapter.Exists<RepositoryEntity>(entity.Id))
                throw new RepositoryAlreadyExistsError(null); // TODO

            await _backendAdapter.Insert(entity);
        }

        public async Task Delete(string repositoryId)
        {
            if(!await _backendAdapter.Exists<RepositoryEntity>(repositoryId))
                throw new RepositoryNotFoundError();

            await _backendAdapter.Delete(new RepositoryEntity() {Id = repositoryId});
        }

        public async Task GrantAccess(string repositoryId, string userId, RepositoryAccessType accessType)
        {
            var repository = await _backendAdapter.Get<RepositoryEntity>(repositoryId);
            if (repository is null)
                throw new RepositoryNotFoundError();

            if (repository.OwnerId == userId) // why granting something? ... he owns the repo :-P
                return;

            await _backendAdapter.Insert(new RepositoryAccessEntity()
            {
                RepositoryId = repositoryId,
                UserId = userId,
                AccessType = accessType
            });
        }

        public async Task RevokeAccess(string repositoryId, string userId)
        {
            if(!await Exists(repositoryId))
                throw new RepositoryNotFoundError();

            await _backendAdapter.Delete(new RepositoryAccessEntity()
            {
                RepositoryId = repositoryId,
                UserId = userId
            });
        }

        public async Task<bool> HasAccess(string repositoryId, string userId, RepositoryAccessType accessType)
        {
            var repository = await _backendAdapter.Get<RepositoryEntity>(repositoryId);
            if(repository is null)
                throw new RepositoryNotFoundError();

            return repository.OwnerId == userId || (await _backendAdapter.Get<RepositoryAccessEntity>(query =>
            {
                query.Where(filter => filter
                    .Equal(nameof(RepositoryAccessEntity.RepositoryId), repositoryId)
                    .And()
                    .Equal(nameof(RepositoryAccessEntity.UserId), userId)
                    .And()
                    .Equal(nameof(RepositoryAccessEntity.AccessType), accessType.ToString())
                );
            }) is not null);
        }
    }
}