using Storagr.Data;

namespace Storagr
{
    public interface IStoragrLock :
        IStoragrExistable,
        IStoragrRunner<StoragrLock>,
        IStoragrCreatable<StoragrLock>,
        IStoragrDeletable<StoragrLock>
    {
        
    }

    public interface IStoragrLockList :
        IStoragrCountable,
        IStoragrEnumerable<StoragrLock, IStoragrLockParams>
    {
        
    }
    
    public interface IStoragrLockProvider :
        IStoragrLockProvider<IStoragrLock, IStoragrLockList>
    {
    }

    public interface IStoragrLockProvider<out TItem, out TList>
        where TItem : IStoragrLock
        where TList : IStoragrLockList
    {
        TItem Lock(string lockIdOrPath);
        TList Locks();
    }
}