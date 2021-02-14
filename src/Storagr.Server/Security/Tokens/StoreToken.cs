using Microsoft.IdentityModel.JsonWebTokens;
using Storagr.Shared.Security;

namespace Storagr.Server.Security.Tokens
{
    public class StoreToken
    {
        [TokenClaim(Name = StoragrConstants.TokenUnqiueId)]
        [TokenClaim(Name = JwtRegisteredClaimNames.Sub)] 
        public string UniqueId { get; set; }
    }
}