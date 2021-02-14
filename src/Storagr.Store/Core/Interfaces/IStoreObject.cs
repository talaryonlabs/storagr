using System.IO;
using Storagr.Shared.Data;

namespace Storagr.Store
{
    public interface IStoreObject :
        IStoreModel<StoreObject>,
        IStoreMeta,
        IStoreDeletable,
        IStoreExistable
    {
        Stream GetDownloadStream();
        Stream GetUploadStream();
    }
}