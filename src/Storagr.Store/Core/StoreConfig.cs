using System;
using System.Net;
using Storagr.Shared;
using Storagr.Shared.Security;

namespace Storagr.Store
{
    [StoragrConfig]
    public class StoreConfig : StoragrOptions<StoreConfig>
    {
        [StoragrConfigValue] public IPEndPoint Listen { get; set; }
        [StoragrConfigValue] public string RootPath { get; set; }
        [StoragrConfigValue] public StoreCacheType Cache { get; set; }
        
        [StoragrConfigValue(IsNamedSize = true)] public int BufferSize { get; set; }
        [StoragrConfigValue(IsNamedDelay = true)] public TimeSpan Expiration { get; set; }
        [StoragrConfigValue(IsNamedDelay = true)] public TimeSpan ScanInterval { get; set; }
    }

    [StoragrConfig("Token")]
    public class TokenConfig
    {
        [StoragrConfigValue] public string Secret { get; set; }
        [StoragrConfigValue] public string Issuer { get; set; }
        [StoragrConfigValue] public string Audience { get; set; }
        
        public static implicit operator StoragrTokenValidationParameters(TokenConfig config) => 
            new StoragrTokenValidationParameters(config.Issuer, config.Audience, config.Secret);
    }

    [StoragrConfig("Redis")]
    public class RedisConfig
    {
        [StoragrConfigValue] public string Host { get; set; }
    }
}