using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storagr.Security
{
    public class AuthenticationRequest
    {
        public string Username;
        public string Password;
    }

    public class AuthenticationResult
    {
        public string Id => Get<string>(AuthenticationResultType.Id);
        public string Username => Get<string>(AuthenticationResultType.Username);
        public string Mail => Get<string>(AuthenticationResultType.Mail);
        public string Role => Get<string>(AuthenticationResultType.Role);
        
        public IEnumerable<string> Roles => Get<IEnumerable<string>>(AuthenticationResultType.Roles);

        public T Get<T>(string type) => (Values ?? new Dictionary<string, object>()).ContainsKey(type) ? (T) Values?[type] : default;

        public readonly Dictionary<string, object> Values = new Dictionary<string, object>();
    }

    public static class AuthenticationResultType
    {
        public const string Id = "id";
        public const string Username = "username";
        public const string Mail = "mail";
        public const string Role = "role";
        public const string Roles = "roles";
    }

    public interface IAuthenticationAdapter
    {
        string Name { get; }

        Task<AuthenticationResult> Authenticate(AuthenticationRequest authenticationRequest);
    }
}