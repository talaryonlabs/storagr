using Storagr.Shared;

namespace Storagr.Client
{
    public interface IStoragrClientAuthenticator
    {
        IStoragrRunner<bool> With(string username, string password);
        IStoragrRunner<bool> With(string token);
    }
}