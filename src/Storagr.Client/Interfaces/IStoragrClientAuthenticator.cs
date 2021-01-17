namespace Storagr.Client
{
    public interface IStoragrClientAuthenticator
    {
        IStoragrClientRunner<bool> With(string username, string password);
        IStoragrClientRunner<bool> With(string token);
    }
}