using System;
using Storagr.Shared.Security;

namespace Storagr.Server
{
    [StoragrConfig("Token")]
    public class TokenOptions : StoragrOptions<TokenOptions>
    {
        [StoragrConfigValue] public string Secret { get; set; }
        [StoragrConfigValue] public string Issuer { get; set; }
        [StoragrConfigValue] public string Audience { get; set; }
        [StoragrConfigValue(IsNamedDelay = true)] public TimeSpan Expiration { get; set; }
        
        public static implicit operator StoragrTokenParameters(TokenOptions options) => 
            new StoragrTokenParameters(options.Issuer, options.Audience, options.Secret);
    }
}