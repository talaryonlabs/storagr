using System.Collections.Generic;
using Storagr.Shared;

namespace Storagr.Server
{
    public interface ICacheService
    {
        ICacheServiceEntry<T> Key<T>(string key);
        ICacheServiceEntry<object> Key(string key);
        IStoragrRunner RemoveMany(IEnumerable<string> keys);
    }
    
    public interface ICacheServiceEntry<T> :
        IStoragrRunner<T>,
        IStoragrExistable,
        IStoragrDeletable
    {
        IStoragrRunner Set(T value);
        IStoragrRunner Refresh(T value);
    }
}