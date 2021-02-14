namespace Storagr.Shared
{
    public interface IStoragrParams<TResult, out TParams> : 
        IStoragrRunner<TResult>
    {
        IStoragrRunner<TResult> With(System.Action<TParams> withParams);
    }
}