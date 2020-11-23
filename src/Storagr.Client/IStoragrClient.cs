using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public struct StoragrLockListOptions
    {
        public int Limit;
        public string PathPattern;
        public string IdPattern;
    }
    
    public interface IStoragrClient : IStoragrUserManagement, IStoragrRepositoryManagement, IStoragrObjectManagement, IStoragrLockManagement
    { 
        string Token { get; }
        StoragrUser User { get; }
        
        Task<bool> Authenticate(string token);
        Task<bool> Authenticate(string username, string password);
    }

    public interface IStoragrUserManagement
    {
        // TODO Task<bool> CreateUser();
        Task<StoragrUser> GetUser(string userId);
        Task<IEnumerable<StoragrUser>> GetUsers();
        // TODO Task<bool> DeleteUser(string userId);
    }

    public interface IStoragrRepositoryManagement
    {
        Task<StoragrRepository> CreateRepository(string repositoryId);
        Task<StoragrRepository> GetRepository(string repositoryId);
        Task<IEnumerable<StoragrRepository>> GetRepositories();
        Task<bool> DeleteRepository(string repositoryId);
    }

    public interface IStoragrObjectManagement
    {
        Task<StoragrAction> BatchObject(string repositoryId, string objectId, StoragrBatchOperation operation);
        Task<StoragrObject> GetObject(string repositoryId, string objectId);
        Task<IEnumerable<StoragrObject>> GetObjects(string repositoryId);
        Task<bool> DeleteObject(string repositoryId, string objectId);
    }

    public interface IStoragrLockManagement
    {
        Task<bool> Lock(string repositoryId, string path);
        Task<bool> Unlock(string repositoryId);
        
        Task<StoragrLock> GetLock(string repositoryId, string lockId);
        Task<IEnumerable<StoragrLock>> GetLocks(string repositoryId) => GetLocks(repositoryId, new StoragrLockListOptions());
        Task<IEnumerable<StoragrLock>> GetLocks(string repositoryId, StoragrLockListOptions listOptions);
    }
}