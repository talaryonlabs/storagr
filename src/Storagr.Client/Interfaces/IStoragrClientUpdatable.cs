namespace Storagr.Client
{
    public interface IStoragrClientUpdatable<TItem, out TParams>
    {
        IStoragrClientParams<TItem, TParams> Update();
    }
}