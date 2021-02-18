using System.Collections.Generic;
using Storagr.Shared.Data;

namespace Storagr.Store
{
    public interface IStoreService
    {
        int BufferSize { get; }
        long UsedSpace { get; }
        long AvailableSpace { get; }

        IStoreObject Object(string objectId);
        IEnumerable<StoreObject> Objects();
    }
}