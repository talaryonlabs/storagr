using System.Collections.Generic;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server
{
    // public interface IStoreAdapter
    // {
    //     Task<bool> Finalize(string repositoryId, string objectId, ulong expectedSize, CancellationToken cancellationToken = default);
    //     
    //     Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId, CancellationToken cancellationToken = default);
    //     Task<StoragrAction> NewUploadAction(string repositoryId, string objectId, CancellationToken cancellationToken = default);
    // }


    public interface IStoreAdapter
    {
        IStoreAdapterRepository Repository(string repositoryId);
        IStoragrEnumerable<StoreRepository> Repositories();
    }

    public interface IStoreAdapterRepository : 
        IStoragrRunner<StoreRepository>,
        IStoragrExistable,
        IStoragrDeletable
    {
        IStoreAdapterObject Object(string objectId);
        IStoragrEnumerable<StoreObject> Objects();
    }

    public interface IStoreAdapterObject :
        IStoragrRunner<StoragrAction>,
        IStoragrExistable,
        IStoragrDeletable
    {
    }

    public interface IStoreAdapterRequest
    {
        IStoreAdapterRequestItem Download();
        IStoreAdapterRequestItem Upload();
    }

    public interface IStoreAdapterRequestItem
    {
        void Objects(IEnumerable<StoreObject> objects);
    }
}