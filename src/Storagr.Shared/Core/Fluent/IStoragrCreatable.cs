namespace Storagr.Shared
{
    public interface IStoragrCreatable<TResult>
    {
        IStoragrRunner<TResult> Create();
    }
    
    public interface IStoragrCreatable<TResult, out TParams>
    {
        IStoragrParams<TResult, TParams> Create();
    }
}