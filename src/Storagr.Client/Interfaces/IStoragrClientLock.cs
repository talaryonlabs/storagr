using Storagr.Client.Params;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrClientLock :
        IStoragrRunner<StoragrLock>,
        IStoragrExistable,
        IStoragrCreatable<StoragrLock>,
        IStoragrDeletable<StoragrLock>
    {
        
    }
    
    public interface IStoragrClientLockList :
        IStoragrListable<StoragrLock, IStoragrLockParams>
    {

    }
}