namespace Storagr.Client
{
    public interface IStoragrClientCreatable<TItem, out TParams>
    {
        IStoragrClientParams<TItem, TParams> Create();
    }
    
    public interface IStoragrClientCreatable<TItem>
    {
        IStoragrClientRunner<TItem> Create();
    }
}