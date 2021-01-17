﻿namespace Storagr.Store
{
    public static class StoreCaching
    {
        public const string CachePrefix = "STORAGR-STORE";
        
        public static string GetTempFileKey(string repositoryId, string objectId) => $"{CachePrefix}:TMP:{repositoryId}:{objectId}";
    }
}