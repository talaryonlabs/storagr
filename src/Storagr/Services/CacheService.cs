using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Storagr.Shared;

namespace Storagr.Services
{
    // STORAGR:{KEY} = list of ids (count)
    // STORAGR:{KEY}:{ID} = object by id
    // STORAGR:{KEY}:{ID}:{SUBKEY} = list of ids (count)
    // STORAGR:{KEY}:{ID}:{SUBKEY}:{SUBID} = object by id
    
    public interface ICacheService
    {
        Task<bool> Exists<TKey>(string keyId);
        Task<bool> Exists<TKey, TSubKey>(string keyId, string subKeyId);

        Task<int> Count<TKey>();
        Task<int> Count<TKey, TSubKey>(string keyId);
        
        Task<TKey> Get<TKey>(string keyId);
        Task<TSubKey> Get<TKey, TSubKey>(string keyId, string subKeyId);

        Task Set<TKey, TData>(string keyId, TData data);
        Task Set<TKey, TSubKey, TData>(string keyId, string subKeyId, TData data);
        
        Task Delete<TKey>(string keyId);
        Task Delete<TKey, TSubKey>(string keyId, string subKeyId);
    }

    public class CacheRegistry : List<string>
    {
        // TODO cached time
    }
    
    public class CacheService : ICacheService
    {
        private static string GetKeyName<TKey>()
        {
            var type = typeof(TKey);
            var attribute = (CacheKeyAttribute)type.GetCustomAttribute(typeof(CacheKeyAttribute));

            return (attribute != null ? attribute.Name : type.Name).ToUpper();
        }

        private const string Prefix = "STORAGR";
        
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _entryOptions;
        private readonly List<string> _registryCache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(IDistributedCache));
            _entryOptions = new DistributedCacheEntryOptions(); // TODO get from config
            _registryCache = new List<string>();
        }

        private async Task<CacheRegistry> GetRegistry<TKey>()
        {
            return (await _cache.GetAsyncObject<CacheRegistry>($"{Prefix}:{GetKeyName<TKey>()}")) ?? new CacheRegistry();
        }

        private Task SetRegistry<TKey>(CacheRegistry registry)
        {
            return _cache.SetAsyncObject($"{Prefix}:{GetKeyName<TKey>()}", registry);
        }
        
        private async Task<CacheRegistry> GetRegistry<TKey, TSubKey>(string keyId)
        {
            return (await _cache.GetAsyncObject<CacheRegistry>(string.Join(":", new[]
            {
                Prefix,
                GetKeyName<TKey>(),
                keyId,
                GetKeyName<TSubKey>()
            }))) ?? new CacheRegistry();
        }
        private Task SetRegistry<TKey, TSubKey>(string keyId, CacheRegistry registry)
        {
            return _cache.SetAsyncObject(string.Join(":", new[]
            {
                Prefix,
                GetKeyName<TKey>(),
                keyId,
                GetKeyName<TSubKey>()
            }), registry ?? new CacheRegistry());
        }

        private Task<TKey> GetObject<TKey>(string keyId)
        {
            return _cache.GetAsyncObject<TKey>(string.Join(":", new[]
            {
                Prefix,
                GetKeyName<TKey>(),
                keyId
            }));
        }
        
        private Task<TSubKey> GetObject<TKey, TSubKey>(string keyId, string subKeyId)
        {
            return _cache.GetAsyncObject<TSubKey>(string.Join(":", new[]
            {
                Prefix,
                GetKeyName<TKey>(),
                keyId,
                GetKeyName<TSubKey>(),
                subKeyId
            }));
        }

        public async Task<bool> Exists<TKey>(string keyId)
        {
            var registry = await GetRegistry<TKey>();
            if (!registry.Contains(keyId))
            {
                return false;
            }

            var item = await GetObject<TKey>(keyId);
            if (item != null) 
                return true;
            
            registry.Remove(keyId);
            await SetRegistry<TKey>(registry);
            return false;
        }

        public async Task<bool> Exists<TKey, TSubKey>(string keyId, string subKeyId)
        {
            var registry = await GetRegistry<TKey, TSubKey>(keyId);
            if (!registry.Contains(subKeyId))
            {
                return false;
            }

            var item = await GetObject<TKey, TSubKey>(keyId, subKeyId);
            if (item != null)
                return true;

            registry.Remove(keyId);
            await SetRegistry<TKey>(registry);
            return false;
        }

        public async Task<int> Count<TKey>()
        {
            var registry = await GetRegistry<TKey>();

            foreach (var keyId in registry.ToList())
                if (await GetObject<TKey>(keyId) == null)
                {
                    registry.Remove(keyId);
                }

            await SetRegistry<TKey>(registry);

            return registry.Count;
        }

        public async Task<int> Count<TKey, TSubKey>(string keyId)
        {
            var registry = await GetRegistry<TKey, TSubKey>(keyId);

            foreach (var subKeyId in registry.ToList())
                if (await GetObject<TKey, TSubKey>(keyId, subKeyId) == null)
                {
                    registry.Remove(subKeyId);
                }

            await SetRegistry<TKey, TSubKey>(keyId, registry);

            return registry.Count;
        }

        public async Task<TKey> Get<TKey>(string keyId)
        {
            var registry = await GetRegistry<TKey>();
            if (!registry.Contains(keyId))
                return default;

            return await GetObject<TKey>(keyId);
        }

        public async Task<TSubKey> Get<TKey, TSubKey>(string keyId, string subKeyId)
        {
            var registry = await GetRegistry<TKey, TSubKey>(keyId);
            if (!registry.Contains(subKeyId))
                return default;

            return await GetObject<TKey, TSubKey>(keyId, subKeyId);
        }

        public async Task Set<TKey, TData>(string keyId, TData data)
        {
            throw new NotImplementedException();
        }

        public async Task Set<TKey, TSubKey, TData>(string keyId, string subKeyId, TData data)
        {
            throw new NotImplementedException();
        }

        public async Task Delete<TKey>(string keyId)
        {
            var registry = await GetRegistry<TKey>();

            if (registry.Contains(keyId))
                registry.Remove(keyId);

            await SetRegistry<TKey>(registry);
            await _cache.RemoveAsync($"{Prefix}:{GetKeyName<TKey>()}:{keyId}");
        }

        public async Task Delete<TKey, TSubKey>(string keyId, string subKeyId)
        {
            var registry = await GetRegistry<TKey, TSubKey>(keyId);

            if (registry.Contains(keyId))
                registry.Remove(keyId);

            await SetRegistry<TKey, TSubKey>(keyId, registry);
            await _cache.RemoveAsync($"{Prefix}:{GetKeyName<TKey>()}:{keyId}:{GetKeyName<TSubKey>()}:{subKeyId}");
        }
    }
}