namespace Storagr.Client.Params
{
    public interface IStoragrLockParams
    {
        IStoragrLockParams Id(string lockId);
        IStoragrLockParams Path(string lockedPath);
        IStoragrLockParams Owner(string owner);
    }
}