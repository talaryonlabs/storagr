using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr
{
    public interface IStoreAdapter
    {
        Task<StoreRepository> Get(string repositoryId);
        Task<StoreObject> Get(string repositoryId, string objectId);
        
        Task<IEnumerable<StoreRepository>> GetAll();
        Task<IEnumerable<StoreObject>> GetAll(string repositoryId);
        
        Task Delete(string repositoryId);
        Task Delete(string repositoryId, string objectId);
        
        Task<bool> Verify(string repositoryId, string objectId, long expectedSize);
        
        Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId);
        Task<StoragrAction> NewUploadAction(string repositoryId, string objectId);
    }
}