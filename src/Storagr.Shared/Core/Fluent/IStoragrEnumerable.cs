using System;
using System.Collections.Generic;

namespace Storagr.Shared
{
    public interface IStoragrEnumerable<TItem> :
        IStoragrRunner<IEnumerable<TItem>>
    {
        IStoragrEnumerable<TItem> Take(int count);
        IStoragrEnumerable<TItem> Skip(int count);
        IStoragrEnumerable<TItem> SkipUntil(string cursor);
    }
    
    public interface IStoragrEnumerable<TItem, out TParams> :
        IStoragrRunner<IEnumerable<TItem>>
    {
        IStoragrEnumerable<TItem, TParams> Take(int count);
        IStoragrEnumerable<TItem, TParams> Skip(int count);
        IStoragrEnumerable<TItem, TParams> SkipUntil(string cursor);
        IStoragrEnumerable<TItem, TParams> Where(Action<TParams> whereParams);
    }
}