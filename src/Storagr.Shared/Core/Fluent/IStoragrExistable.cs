namespace Storagr.Shared
{
    public interface IStoragrExistable
    {
        IStoragrRunner<bool> Exists();
    }
}