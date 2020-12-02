using System;
using System.Net;
using Microsoft.Extensions.Options;
using Storagr.Shared;
using Storagr.Shared.Security;

namespace Storagr
{
    public enum StoragrBackendType
    {
        Sqlite
    }

    public enum StoragrStoreType
    {
        Storagr
    }
    
    public enum StoragrCacheType
    {
        Memory,
        Redis
    }
    
    [StoragrConfig]
    public class StoragrCoreConfig
    {
        [StoragrConfigValue] public IPEndPoint Listen { get; set; }
        
        [StoragrConfigValue] public StoragrBackendType Backend { get; set; }
        [StoragrConfigValue] public StoragrStoreType Store { get; set; }
        [StoragrConfigValue] public StoragrCacheType Cache { get; set; }
    }
    
    [StoragrConfig("Token")]
    public class TokenConfig : StoragrOptions<TokenConfig>
    {
        [StoragrConfigValue] public string Secret { get; set; }
        [StoragrConfigValue] public string Issuer { get; set; }
        [StoragrConfigValue] public string Audience { get; set; }
        [StoragrConfigValue(IsNamedDelay = true)] public TimeSpan Expiration { get; set; }
        
        public static implicit operator StoragrTokenParameters(TokenConfig config) => 
            new StoragrTokenParameters(config.Issuer, config.Audience, config.Secret);
    }
    
    [StoragrConfig("Memory")]
    public class MemoryCacheConfig
    {
        [StoragrConfigValue(IsNamedSize = true)] public long SizeLimit { get; set; }
    }
    
    [StoragrConfig("Redis")]
    public class RedisCacheConfig
    {
        [StoragrConfigValue] public string Host { get; set; }
    }
    
    [StoragrConfig("StoragrStore")]
    public class StoragrStoreConfig
    {
        [StoragrConfigValue] public string Host { get; set; }
    }
}