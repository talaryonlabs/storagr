using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr
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
        
        Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId);
        Task<StoragrAction> NewUploadAction(string repositoryId, string objectId);
    }
}