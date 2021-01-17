using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrLockParams
    {
        IStoragrLockParams Id(string lockId);
        IStoragrLockParams Path(string lockedPath);
    }
    
    public interface IStoragrClientLock :
        IStoragrClientRunner<StoragrLock>,
        IStoragrClientCreatable<StoragrLock>,
        IStoragrClientDeletable<StoragrLock>
    {

    }

    public interface IStoragrClientLockList :
        IStoragrClientList<StoragrLock, IStoragrLockParams>
    {

    }
}