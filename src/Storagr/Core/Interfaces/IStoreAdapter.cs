using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr
{
    public interface IStoreAdapter
    {        
        Task<StoreRepository> Get(string repositoryId, CancellationToken cancellationToken = default);
        Task<StoreObject> Get(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<StoreRepository>> GetAll(CancellationToken cancellationToken = default);
        Task<IEnumerable<StoreObject>> GetAll(string repositoryId, CancellationToken cancellationToken = default);
        
        Task Delete(string repositoryId, CancellationToken cancellationToken = default);
        Task Delete(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        
        Task<bool> Finalize(string repositoryId, string objectId, long expectedSize, CancellationToken cancellationToken = default);
        
        Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        Task<StoragrAction> NewUploadAction(string repositoryId, string objectId, CancellationToken cancellationToken = default);
    }
}