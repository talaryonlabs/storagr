namespace Storagr.Server
{
    public interface ILockParams
    {
        ILockParams Id(string lockId);
        ILockParams Path(string lockedPath);
        ILockParams Owner(string owner);
    }
}