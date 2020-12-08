using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrClient : IDisposable
    { 
        bool IsAuthenticated { get; }
        string Token { get; }
        StoragrUser User { get; }
        
        Task<bool> Authenticate(string token);
        Task<bool> Authenticate(string username, string password);
        
        /**
         * Users
         */
        // TODO Task<StoragrUser> CreateUser();
        Task<StoragrUser> GetUser(string userId);
        Task<IEnumerable<StoragrUser>> GetUsers();
        // TODO Task DeleteUser(string userId);

        /**
         * Logs
         */
        Task<StoragrLogList> GetLogs(StoragrLogListOptions options);
        
        /**
         * Repositories
         */
        Task<StoragrRepository> CreateRepository(string repositoryId, string ownerId, long sizeLimit);
        Task<StoragrRepository> GetRepository(string repositoryId);
        Task<IEnumerable<StoragrRepository>> GetRepositories();
        Task DeleteRepository(string repositoryId);
        
        /**
         * Objects
         */
        Task<StoragrBatchObject> BatchObject(string repositoryId, StoragrBatchOperation operation, StoragrObject obj);
        Task<IEnumerable<StoragrBatchObject>> BatchObjects(string repositoryId, StoragrBatchOperation operation, IEnumerable<StoragrObject> objList);
        
        Task<StoragrObject> GetObject(string repositoryId, string objectId);
        Task<StoragrObjectList> GetObjects(string repositoryId) => GetObjects(repositoryId, StoragrObjectListOptions.Empty);
        Task<StoragrObjectList> GetObjects(string repositoryId, StoragrObjectListOptions options);
        Task DeleteObject(string repositoryId, string objectId);
        
        /**
         * Locking
         */
        Task<StoragrLock> CreateLock(string repositoryId, string path);

        Task<StoragrLock> Unlock(string repositoryId, string lockId) => DeleteLock(repositoryId, lockId, false);
        Task<StoragrLock> DeleteLock(string repositoryId, string lockId, bool force);
        
        Task<StoragrLock> GetLock(string repositoryId, string lockId);
        Task<StoragrLockList> GetLocks(string repositoryId) => GetLocks(repositoryId, StoragrLockListOptions.Empty);
        Task<StoragrLockList> GetLocks(string repositoryId, StoragrLockListOptions options);
    }
}