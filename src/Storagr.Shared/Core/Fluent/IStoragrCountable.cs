namespace Storagr.Shared
{
    public interface IStoragrCountable
    {
        IStoragrRunner<int> Count();
    }
}