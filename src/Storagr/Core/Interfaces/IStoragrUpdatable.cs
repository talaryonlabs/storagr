namespace Storagr
{
    public interface IStoragrUpdatable<TResult, out TParams>
    {
        IStoragrParams<TResult, TParams> Update();
    }
}