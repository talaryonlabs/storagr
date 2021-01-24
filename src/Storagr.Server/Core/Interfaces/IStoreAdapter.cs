using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Data;

namespace Storagr.Server
{
    public interface IStoreAdapter
    {        
        Task<StoreRepository> Get(string repositoryId, CancellationToken cancellationToken = default);
        Task<StoreObject> Get(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<StoreRepository>> GetAll(CancellationToken cancellationToken = default);
        Task<IEnumerable<StoreObject>> GetAll(string repositoryId, CancellationToken cancellationToken = default);
        
        Task Delete(string repositoryId, CancellationToken cancellationToken = default);
        Task Delete(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        
        Task<bool> Finalize(string repositoryId, string objectId, ulong expectedSize, CancellationToken cancellationToken = default);
        
        Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        Task<StoragrAction> NewUploadAction(string repositoryId, string objectId, CancellationToken cancellationToken = default);
    }


    public interface IStoreAdapter2
    {
        IStoreAdapterRepository Repository(string repositoryId);
        IStoragrEnumerable<StoreRepository> Repositories();
    }

    public interface IStoreAdapterRepository : 
        IStoragrRunner<StoreRepository>
    {
        IStoragrRunner<bool> Exists();

        IStoreAdapterObject Object(string objectId);
        IStoragrEnumerable<StoreObject> Objects();
    }

    public interface IStoreAdapterObject :
        IStoragrRunner<StoreObject>,
        IStoragrDeletable<bool>
    {
        IStoragrRunner<bool> Exists();
    }
}