using Storagr.Shared;
using Storagr.Shared.Security;

namespace Storagr
{
    public class TokenServiceOptions : StoragrOptions<TokenServiceOptions>
    {
        public StoragrTokenValidationParameters ValidationParameters { get; set; }
        public string Secret { get; set; }
        public int Expiration { get; set; }
    }
}