using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Storagr.Shared.Security;

namespace Storagr.Security.Tokens
{
    public class UserToken
    {
        [StoragrClaim(Name = StoragrConstants.TokenUnqiueId)] 
        [StoragrClaim(Name = JwtRegisteredClaimNames.Sub)]
        public string UserId { get; set; }
        
        [StoragrClaim(Name = ClaimTypes.Role)] 
        public string Role { get; set; }
    }
}