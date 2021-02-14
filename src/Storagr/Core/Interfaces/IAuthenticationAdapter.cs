using System.Threading.Tasks;

namespace Storagr
{
    public interface IAuthenticationResult
    {
        string Id { get; set; }
        string Username { get; set; }
    }

    public interface IAuthenticationAdapter
    {
        /// <summary>
        /// Returns the name of the adapter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Authenticates with a token.
        /// </summary>
        /// <param name="token">Token used to authenticate.</param>
        /// <returns>Successful authenticated result, otherwise null.</returns>
        Task<IAuthenticationResult> Authenticate(string token);
        
        /// <summary>
        /// Authenticates with username and password.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Successful authenticated result, otherwise null.</returns>
        Task<IAuthenticationResult> Authenticate(string username, string password);
    }
}