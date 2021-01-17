using System;

namespace Storagr.Client
{
    public interface IStoragrClientParams<TItem, out TParams> : IStoragrClientRunner<TItem>
    {
        IStoragrClientRunner<TItem> With(Action<TParams> withParams);
    }
}