namespace Storagr.Shared
{
    public interface IStoragrUpdatable<TResult, out TParams>
    {
        IStoragrParams<TResult, TParams> Update();
    }
}