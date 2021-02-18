using System.Net;
using Storagr.Shared.Security;

namespace Storagr.Store
{
    [StoragrConfig]
    public class StoreConfig : StoragrOptions<StoreConfig>
    {
        [StoragrConfigValue] public IPEndPoint Listen { get; set; }
        [StoragrConfigValue] public string RootPath { get; set; }
        [StoragrConfigValue(IsNamedSize = true)] public int BufferSize { get; set; }
    }

    [StoragrConfig("Token")]
    public class TokenConfig
    {
        [StoragrConfigValue] public string Secret { get; set; }
        [StoragrConfigValue] public string Issuer { get; set; }
        [StoragrConfigValue] public string Audience { get; set; }
        
        public static implicit operator StoragrTokenParameters(TokenConfig config) => 
            new(config.Issuer, config.Audience, config.Secret);
    }
}