using System;

namespace Storagr
{
    public interface IStoragrListable<TItem> :
        IStoragrRunner<IStoragrList<TItem>>
    {
        IStoragrListable<TItem> Take(int count);
        IStoragrEnumerable<TItem> Skip(int count);
        IStoragrListable<TItem> SkipUntil(string cursor);
    }

    public interface IStoragrListable<TItem, out TParams> :
        IStoragrRunner<IStoragrList<TItem>>
    {
        IStoragrListable<TItem, TParams> Take(int count);
        IStoragrListable<TItem, TParams> Skip(int count);
        IStoragrListable<TItem, TParams> SkipUntil(string cursor);
        IStoragrListable<TItem, TParams> Where(Action<TParams> whereParams);
    }
}