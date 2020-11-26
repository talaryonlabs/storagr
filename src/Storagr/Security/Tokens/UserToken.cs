using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Storagr.Shared.Security;

namespace Storagr.Security.Tokens
{
    public class UserToken
    {
        [StoragrTokenMember(Name = "uid", ClaimType = JwtRegisteredClaimNames.Sub)] public string UserId;
        [StoragrTokenMember(Name = "role", ClaimType = ClaimTypes.Role)] public string Role;
    }
}