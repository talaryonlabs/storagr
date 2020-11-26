using Microsoft.IdentityModel.JsonWebTokens;
using Storagr.Shared.Security;

namespace Storagr.Security.Tokens
{
    public class StoreToken
    {
        [StoragrTokenMember(Name = "uid", ClaimType = JwtRegisteredClaimNames.Sub)] public string UniqueId { get; set; }
    }
}