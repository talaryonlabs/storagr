﻿using System.Net;

namespace Storagr.Server
{
    public enum StoragrBackendType
    {
        Sqlite,
        MySql
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
}