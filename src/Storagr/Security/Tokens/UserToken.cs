using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Storagr.Shared.Security;

namespace Storagr.Security.Tokens
{
    public class UserToken
    {
        [TokenClaim(Name = StoragrConstants.TokenUnqiueId)] 
        [TokenClaim(Name = JwtRegisteredClaimNames.Sub)]
        public string UserId { get; set; }
        
        [TokenClaim(Name = ClaimTypes.Role)] 
        public string Role { get; set; }
    }
}