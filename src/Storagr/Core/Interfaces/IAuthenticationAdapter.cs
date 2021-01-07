using System.Threading;
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
        string Name { get; }
        
        Task<IAuthenticationResult> Authenticate(string token, CancellationToken cancellationToken = default);
        
        Task<IAuthenticationResult> Authenticate(string username, string password, CancellationToken cancellationToken = default);
    }
}