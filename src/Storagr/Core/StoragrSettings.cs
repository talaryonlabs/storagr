namespace Storagr
{
    public enum StoragrCacheType
    {
        Memory,
        Redis
    }

    public enum StoragrBackendType
    {
        Sqlite
    }

    public enum StoragrStoreType
    {
        Local
    }
    
    public class StoragrSettings
    {
        public StorageTokenSettings TokenSettings { get; set; } = new StorageTokenSettings();
        public StoragrCacheSettings CacheSettings { get; set; } = new StoragrCacheSettings();
        public StoragrStoreSettings StoreSettings { get; set; } = new StoragrStoreSettings();
        public StoragrBackendSettings BackendSettings { get; set; } = new StoragrBackendSettings();
    }

    public class StoragrCacheSettings
    {
        public StoragrCacheType Type { get; set; }
        public int SizeLimit { get; set; }
        public string Host { get; set; }
    }

    public class StoragrStoreSettings
    {
        public StoragrStoreType Type { get; set; }
        public string RootPath { get; set; }
    }

    public class StoragrBackendSettings
    {
        public StoragrBackendType Type { get; set; }
        public string DataSource { get; set; }
    }
    
    public class StorageTokenSettings
    {
        public string Secret { get; set; }
        public int Expiration { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}