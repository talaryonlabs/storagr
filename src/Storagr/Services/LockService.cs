using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Storagr.Data;
using Storagr.Data.Entities;

namespace Storagr.Services
{
    public interface ILockService
    {
        Task<LockEntity> Create(string repositoryId, string path);
        
        Task<LockEntity> Get(string repositoryId, string lockId);
        Task<LockEntity> GetByPath(string repositoryId, string path);
        Task<IEnumerable<LockEntity>> GetAll(string repositoryId) => 
            GetAll(repositoryId, -1, null, null, null);
        Task<IEnumerable<LockEntity>> GetAll(string repositoryId, int limit, string cursor) =>
            GetAll(repositoryId, limit, cursor, null, null);
        Task<IEnumerable<LockEntity>> GetAll(string repsitoryId, int limit, string cursor, string lockIdPattern, string pathPattern);

        Task Delete(string repositoryId, string lockId);
        Task DeleteAll(string repositoryId);
    }
    
    public class LockService : ILockService
    {
        private readonly IBackendAdapter _backend;
        private readonly IUserService _userService;

        public LockService(IBackendAdapter backend, IUserService userService)
        {
            _backend = backend;
            _userService = userService;
        }

        public async Task<LockEntity> Create(string repositoryId, string path)
        {
            var repository = await _backend.Get<RepositoryEntity>(repositoryId);
            if (repository == null)
            {
                throw null; // TODO RepositoryNotFound! make exception
            }
            
            var existingLock = _backend.Get<LockEntity>(q =>
            {
                q.Where(f => f.Equal("RepositoryId", repositoryId).And().Equal("Path", path));
            });
            if (existingLock != null)
            {
                throw null; // TODO Lock exists! make exception
            }
            
            var user = await _userService.GetAuthenticatedUser();
            var entity = new LockEntity()
            {
                LockId = StoragrHelper.UUID(),
                RepositoryId = repositoryId,
                OwnerId = user.UserId,
                Path = path,
                LockedAt = DateTime.Now,
                
                Repository = repository,
                Owner = user
            };
            await _backend.Insert(entity);

            return entity;
        }

        public async Task<LockEntity> Get(string repositoryId, string lockId)
        {
            var lockEntity = await _backend.Get<LockEntity>(q =>
            {
                q.Where(f => f.Equal("RepositoryId", repositoryId).And().Equal("LockId", lockId));
            });
            lockEntity.Owner = await _backend.Get<UserEntity>(lockEntity.OwnerId);
            lockEntity.Repository = await _backend.Get<RepositoryEntity>(lockEntity.RepositoryId);

            return lockEntity;
        }

        public async Task<LockEntity> GetByPath(string repositoryId, string path)
        {
            var lockEntity = await _backend.Get<LockEntity>(q =>
            {
                q.Where(f => f.Equal("RepositoryId", repositoryId).And().Equal("Path", path.Trim()));
            });
            lockEntity.Owner = await _backend.Get<UserEntity>(lockEntity.OwnerId);
            lockEntity.Repository = await _backend.Get<RepositoryEntity>(lockEntity.RepositoryId);

            return lockEntity;
        }
        
        public async Task<IEnumerable<LockEntity>> GetAll(string repositoryId, int limit, string cursor, string lockIdPattern, string pathPattern)
        {
            var repository = await _backend.Get<RepositoryEntity>(repositoryId);
            var users = (await _backend.GetAll<UserEntity>()).ToList();
            var locks = (await _backend.GetAll<LockEntity>(x =>
            {
                x.Where(f =>
                {
                    f.Equal("RepositoryId", repositoryId);

                    if (!string.IsNullOrEmpty(lockIdPattern))
                    {
                        f.And().Like("LockId", $"%{lockIdPattern}%");
                    }

                    if (!string.IsNullOrEmpty(pathPattern))
                    {
                        f.And().Like("Path", $"%{pathPattern}%");
                    }
                });
            })).ToList();

            if (!locks.Any())
            {
                return locks;
            }

            if (!string.IsNullOrEmpty(cursor))
                locks = locks.SkipWhile(v => v.LockId != cursor).ToList();

            if (limit > 0)
                locks = locks.Take(limit).ToList();

            foreach (var lockEntity in locks)
            {
                lockEntity.Repository = repository;
                lockEntity.Owner = users.Find(v => v.UserId == lockEntity.OwnerId);
            }

            return locks;
        }

        public async Task Delete(string repositoryId, string lockId)
        {
            var obj = await Get(repositoryId, lockId);
            
            await _backend.Delete(obj);
        }

        public async Task DeleteAll(string repositoryId)
        {
            var list = await (this as ILockService).GetAll(repositoryId);

            await _backend.Delete(list);
        }
    }
}