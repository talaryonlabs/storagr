using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrClient : IDisposable
    {
        bool IsAuthenticated { get; }
        string Token { get; }
        StoragrUser User { get; }

        Task<bool> Authenticate(string token, CancellationToken cancellationToken = default);

        Task Authenticate(string username, string password, CancellationToken cancellationToken = default);


        /**
         * Users
         */
        Task<StoragrUser> CreateUser(StoragrUser user, CancellationToken cancellationToken = default) =>
            CreateUser(user, null, cancellationToken);
        Task<StoragrUser> CreateUser(StoragrUser user, string newPassword = null, CancellationToken cancellationToken = default);
        Task<StoragrUser> GetUser(string userId, CancellationToken cancellationToken = default);

        Task<StoragrUserList> GetUsers(CancellationToken cancellationToken = default) => GetUsers(new StoragrUserListArgs(), cancellationToken);
        Task<StoragrUserList> GetUsers(StoragrUserListArgs listArgs, CancellationToken cancellationToken = default);

        Task DeleteUser(string userId, CancellationToken cancellationToken = default);

        /**
         * Logs
         */
        Task<StoragrLogList> GetLogs(StoragrLogQuery options, CancellationToken cancellationToken = default);

        /**
         * Repositories
         */
        Task<StoragrRepository> CreateRepository(StoragrRepository repository, CancellationToken cancellationToken = default);

        Task<StoragrRepository> GetRepository(string repositoryId, CancellationToken cancellationToken = default);
        Task<StoragrRepositoryList> GetRepositories(CancellationToken cancellationToken = default) => GetRepositories(new StoragrRepositoryListArgs(), cancellationToken);
        Task<StoragrRepositoryList> GetRepositories(StoragrRepositoryListArgs listArgs, CancellationToken cancellationToken = default);
        Task DeleteRepository(string repositoryId, CancellationToken cancellationToken = default);

        /**
         * Objects
         */
        Task<StoragrBatchObject> BatchObject(string repositoryId, StoragrBatchOperation operation,
            StoragrObject obj, CancellationToken cancellationToken = default);

        Task<IEnumerable<StoragrBatchObject>> BatchObjects(string repositoryId, StoragrBatchOperation operation,
            IEnumerable<StoragrObject> objList, CancellationToken cancellationToken = default);

        Task<StoragrObject> GetObject(string repositoryId, string objectId, CancellationToken cancellationToken = default);

        Task<StoragrObjectList> GetObjects(string repositoryId, CancellationToken cancellationToken = default) =>
            GetObjects(repositoryId, new StoragrObjectListQuery(), cancellationToken);

        Task<StoragrObjectList> GetObjects(string repositoryId, StoragrObjectListQuery listQuery, CancellationToken cancellationToken = default);
        Task DeleteObject(string repositoryId, string objectId, CancellationToken cancellationToken = default);

        /**
         * Locking
         */
        Task<StoragrLock> Lock(string repositoryId, string path, CancellationToken cancellationToken = default);

        Task<StoragrLock> Unlock(string repositoryId, string lockId, CancellationToken cancellationToken = default) => Unlock(repositoryId, lockId, false, cancellationToken);
        Task<StoragrLock> Unlock(string repositoryId, string lockId, bool force, CancellationToken cancellationToken = default);

        Task<StoragrLock> GetLock(string repositoryId, string lockId, CancellationToken cancellationToken = default);
        Task<StoragrLockList> GetLocks(string repositoryId) => GetLocks(repositoryId, new StoragrLockListArgs());
        Task<StoragrLockList> GetLocks(string repositoryId, StoragrLockListArgs listArgs, CancellationToken cancellationToken = default);
    }
}