namespace Storagr.Server
{
    public static class StoragrCaching
    {
        public const string CachePrefix = "STORAGR";
        
        public static string GetUserKey(string userId) => $"{CachePrefix}:USER:{userId}";
        public static string GetUserCountKey() => $"{CachePrefix}:USERS";
        
        public static string GetRepositoryKey(string repositoryId) => $"{CachePrefix}:REPOSITORY:{repositoryId}";
        public static string GetRepositoryCountKey() => $"{CachePrefix}:REPOSITORIES";
        
        public static string GetObjectKey(string repositoryId, string objectId) => $"{CachePrefix}:OBJECT:{repositoryId}:{objectId}";
        public static string GetObjectCountKey(string repositoryId) => $"{CachePrefix}:OBJECTS:{repositoryId}";
        
        public static string GetLockKey(string repositoryId, string lockIdOrPath) => $"{CachePrefix}:LOCK:{repositoryId}:{lockIdOrPath}";
        public static string GetLockCountKey(string repositoryId) => $"{CachePrefix}:LOCKS:{repositoryId}";
    }
}