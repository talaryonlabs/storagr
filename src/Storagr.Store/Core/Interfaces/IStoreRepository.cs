using System.Collections.Generic;
using Storagr.Shared.Data;

namespace Storagr.Store
{
    public interface IStoreRepository :
        IStoreModel<StoreRepository>,
        IStoreMeta,
        IStoreMetaName,
        IStoreDeletable,
        IStoreExistable
    {
        IStoreRepository CreateIfNotExists();
        IStoreRepository SetName(string name);
        
        IStoreObject Object(string objectId);
        IEnumerable<IStoreObject> Objects();
    }
}