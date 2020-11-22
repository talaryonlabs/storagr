using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.IO
{
    public interface IStoreAdapter
    {
        Task<bool> Verify(string repositoryId, string objectId, long expectedSize);
        
        Task<StoreRepository> Get(string repositoryId);
        Task<IEnumerable<StoreRepository>> GetAll();
        Task Delete(string repositoryId);
        
        Task<StoreObject> Get(string repositoryId, string objectId);
        Task<IEnumerable<StoreObject>> GetAll(string repositoryId);
        Task Delete(string repositoryId, string objectId);
        
        Task<StoragrAction> NewDownloadRequest(string repositoryId, string objectId);
        Task<StoragrAction> NewUploadRequest(string repositoryId, string objectId);
    }
}