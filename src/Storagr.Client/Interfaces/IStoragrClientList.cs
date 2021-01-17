using System;
using Storagr.Shared;

namespace Storagr.Client
{
    public interface IStoragrClientList<TItem, out TParams> : 
        IStoragrClientRunner<IStoragrList<TItem>>
    {
        IStoragrClientList<TItem, TParams> Take(int count);
        IStoragrClientList<TItem, TParams> Skip(int count);
        IStoragrClientList<TItem, TParams> SkipUntil(string cursor);
        IStoragrClientList<TItem, TParams> Where(Action<TParams> whereParams);
    }
}