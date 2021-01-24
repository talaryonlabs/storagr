using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Data;

namespace Storagr.Client
{
    public interface IStoragrClient :
        IStoragrRepositoryProvider,
        IStoragrUserProvider,
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
    }
    
    public interface IStoragrClientRequest
    {
        Task<TResponse> Send<TResponse>(string uri, HttpMethod method, CancellationToken cancellationToken = default);
        Task<TResponse> Send<TResponse, TRequestData>(string uri, HttpMethod method, TRequestData requestData, CancellationToken cancellationToken = default);
    }
}