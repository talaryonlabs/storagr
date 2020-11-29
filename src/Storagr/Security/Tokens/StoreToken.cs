using Microsoft.IdentityModel.JsonWebTokens;
using Storagr.Shared.Security;

namespace Storagr.Security.Tokens
{
    public class StoreToken
    {
        [StoragrClaim(Name = StoragrConstants.TokenUnqiueId)]
        [StoragrClaim(Name = JwtRegisteredClaimNames.Sub)] 
        public string UniqueId { get; set; }
    }
}