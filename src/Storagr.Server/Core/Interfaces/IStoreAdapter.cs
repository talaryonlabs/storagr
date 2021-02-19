using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server
{
    public interface IStoreAdapter
    {
        IStoragrRunner<StoreInformation> Info();

        IStoreAdapterObject Object(string objectId);
        //IStoragrEnumerable<StoreObject> Objects();
    }

    public interface IStoreAdapterObject :
        IStoragrRunner<StoreObject>,
        IStoragrExistable,
        IStoragrDeletable
    {
        IStoragrRunner<StoragrAction> Download();
        IStoragrRunner<StoragrAction> Upload();
    }
}