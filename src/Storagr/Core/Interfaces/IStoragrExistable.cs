namespace Storagr
{
    public interface IStoragrExistable
    {
        IStoragrRunner<bool> Exists();
    }
}