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
        // TODO Task<bool> CreateUser();
        Task<StoragrUser> GetUser(string userId);
        Task<IEnumerable<StoragrUser>> GetUsers();
        // TODO Task<bool> DeleteUser(string userId);

        /**
         * Logs
         */
        Task<StoragrLogList> GetLogs(StoragrLogListOptions options);
        
        /**
         * Repositories
         */
        Task<StoragrRepository> CreateRepository(string repositoryId);
        Task<StoragrRepository> GetRepository(string repositoryId);
        Task<IEnumerable<StoragrRepository>> GetRepositories();
        Task<bool> DeleteRepository(string repositoryId);
        
        /**
         * Objects
         */
        Task<StoragrAction> BatchObject(string repositoryId, string objectId, StoragrBatchOperation operation);
        Task<StoragrObject> GetObject(string repositoryId, string objectId);
        Task<StoragrObjectList> GetObjects(string repositoryId) => GetObjects(repositoryId, StoragrObjectListOptions.Empty);
        Task<StoragrObjectList> GetObjects(string repositoryId, StoragrObjectListOptions options);
        Task<bool> DeleteObject(string repositoryId, string objectId);
        
        /**
         * Locking
         */
        Task<bool> Lock(string repositoryId, string path);
        Task<bool> Unlock(string repositoryId);
        
        Task<StoragrLock> GetLock(string repositoryId, string lockId);
        Task<StoragrLockList> GetLocks(string repositoryId) => GetLocks(repositoryId, StoragrLockListOptions.Empty);
        Task<StoragrLockList> GetLocks(string repositoryId, StoragrLockListOptions options);
    }
}