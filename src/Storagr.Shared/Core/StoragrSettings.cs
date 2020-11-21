using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

namespace Storagr.Shared
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
        Storagr
    }
    
    public class StoragrSettings
    {
        public StorageTokenSettings TokenSettings { get; private set; }
        public StoragrCacheSettings CacheSettings { get; private set; }
        public StoragrStoreSettings StoreSettings { get; private set; }
        public StoragrBackendSettings BackendSettings { get; private set; }

        public StoragrSettings(IConfiguration configuration)
        {
            var storagrSection = configuration.GetSection("Storagr");
            if(!storagrSection.Exists())
                throw new KeyNotFoundException();

            if(configuration.GetSection("Storagr:Cache").Exists()) ReadCacheSettings(configuration);
            if(configuration.GetSection("Storagr:Store").Exists()) ReadStoreSettings(configuration);
            if(configuration.GetSection("Storagr:Token").Exists()) ReadTokenSettings(configuration);
            if(configuration.GetSection("Storagr:Backend").Exists()) ReadBackendSettings(configuration);
        }

        private void ReadCacheSettings(IConfiguration configuration)
        {
            CacheSettings = new StoragrCacheSettings()
            {
                Type = Enum.Parse<StoragrCacheType>( configuration["STORAGR_CACHE_TYPE"] ?? configuration["Storagr:Cache:Type"], true),
                SizeLimit = (int)StoragrHelper.ParseNamedSize(configuration["STORAGR_CACHE_SIZELIMIT"] ?? configuration["Storagr:Cache:SizeLimit"]),
                Host = configuration["STORAGR_CACHE_HOST"] ?? configuration["Storagr:Cache:Host"],
            };
        }
        
        private void ReadBackendSettings(IConfiguration configuration)
        {
            BackendSettings = new StoragrBackendSettings()
            {
                DataSource = configuration["STORAGR_BACKEND_DATASOURCE"] ?? configuration["Storagr:Backend:DataSource"]
            };
            if (configuration.GetSection("Storagr:Backend:Type").Exists())
            {
                BackendSettings.Type = Enum.Parse<StoragrBackendType>(
                    configuration["STORAGR_BACKEND_TYPE"] ?? configuration["Storagr:Backend:Type"],
                    true
                );
            }
        }
        
        private void ReadStoreSettings(IConfiguration configuration)
        {
            StoreSettings = new StoragrStoreSettings()
            {
                RootPath = configuration["STORAGR_STORE_ROOTPATH"] ?? configuration["Storagr:Store:RootPath"]
            };
            
            if (configuration.GetSection("Storagr:Store:Type").Exists())
            {
                StoreSettings.Type = Enum.Parse<StoragrStoreType>(
                    configuration["STORAGR_STORE_TYPE"] ?? configuration["Storagr:Store:Type"],
                    true
                );
            }
        }
        
        private void ReadTokenSettings(IConfiguration configuration)
        {
            TokenSettings = new StorageTokenSettings()
            {
                Secret = configuration["STORAGR_TOKEN_SECRET"] ?? configuration["Storagr:Token:Secret"],
                Issuer = configuration["STORAGR_TOKEN_ISSUER"] ?? configuration["Storagr:Token:Issuer"],
                Audience = configuration["STORAGR_TOKEN_AUDIENCE"] ?? configuration["Storagr:Token:Audience"],
            };
            
            if (configuration.GetSection("Storagr:Token:Expiration").Exists())
            {
                TokenSettings.Expiration = int.Parse(
                    configuration["STORAGR_TOKEN_EXPIRATION"] ?? configuration["Storagr:Token:Expiration"]
                );
            }
        }
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