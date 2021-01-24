using System.IO;
using Storagr.Data;

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