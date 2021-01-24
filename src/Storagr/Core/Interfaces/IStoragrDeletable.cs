namespace Storagr
{
    public interface IStoragrDeletable
    {
        IStoragrRunner Delete(bool force = false);
    }
    
    public interface IStoragrDeletable<TResult>
    {
        IStoragrRunner<TResult> Delete(bool force = false);
    }
}