using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;

namespace Storagr.Client
{
    public interface IStoragrClient :
        IDisposable
    {
        string Token { get; }
        string Hostname { get; }
        string Protocol { get; }
        short Port { get; }
        
        IStoragrClient UseToken(string token);
        IStoragrClient UseHostname(string hostname);
        
        IStoragrClientAuthenticator Authenticate();

        IStoragrClientLogList Logs();

        IStoragrClientUser User(string userIdOrName);
        IStoragrClientUserList Users();
        
        IStoragrClientRepository Repository(string repositoryIdOrName);
        IStoragrClientRepositoryList Repositories();
    }
    
    public interface IStoragrClientRequest
    {
        Task<TResponse> Send<TResponse>(string uri, HttpMethod method, CancellationToken cancellationToken = default);
        Task<TResponse> Send<TResponse, TRequestData>(string uri, HttpMethod method, TRequestData requestData, CancellationToken cancellationToken = default);
    }
}