namespace Storagr
{
    public interface IStoragrLockParams
    {
        IStoragrLockParams Id(string lockId);
        IStoragrLockParams Path(string lockedPath);
    }
}