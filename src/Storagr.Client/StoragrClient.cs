using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public class StoragrClient : IStoragrClient, IStoragrClientRequest
    {
        public short Port { get; private set; }
        public string Hostname { get;private set; }
        public string Protocol { get;private set; }
        public string Token { get;private set; }
        
        private readonly HttpClient _httpClient;
        private readonly StoragrMediaType _mediaType;
        private readonly StoragrClientOptions _options;

        [ActivatorUtilitiesConstructor]
        internal StoragrClient(IOptions<StoragrClientOptions> options, IHttpClientFactory clientFactory)
            : this(clientFactory.CreateClient())
        {
            _options = options.Value;

            UseHostname(_options.Host);
            UseToken(_options.Token);
        }
        
        public StoragrClient(HttpClient httpClient)
        {
            _mediaType = new StoragrMediaType();
            
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Accept", $"{_mediaType.MediaType.Value}; charset=utf-8"); // application/vnd.git-lfs+json
        }

        async Task<TResponse> IStoragrClientRequest.Send<TResponse>(string uri, HttpMethod method, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(method, uri);
            if (Token is not null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw (StoragrError) responseData;

            return (TResponse) (object) responseData;
        }
        async Task<TResponse> IStoragrClientRequest.Send<TResponse, TRequestData>(string uri, HttpMethod method, TRequestData requestData, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new ByteArrayContent(
                    StoragrHelper.SerializeObject(requestData)
                )
            };
            if (Token is not null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
            request.Content.Headers.Add("Content-Type", $"{_mediaType.MediaType.Value}; charset=utf-8"); // application/vnd.git-lfs+json

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw (StoragrError) responseData;

            return (TResponse) (object) responseData;
        }

        public IStoragrClient UseToken(string token)
        {
            Token = token;
            return this;
        }

        public IStoragrClient UseHostname(string hostname)
        {
            (Protocol, Hostname, Port) = StoragrHelper.ParseHostname(hostname);
            _httpClient.BaseAddress = new Uri($"{(Protocol ?? _options.DefaultProtocol)}://{Hostname}:{(Port > 0 ? Port : _options.DefaultPort)}/v1/");
            return this;
        }

        public IStoragrClientAuthenticator Authenticate()
        {
            return new StoragrClientAuthenticator(this, this);
        }

        public IStoragrClientList<StoragrLog, IStoragrLogParams> Logs()
        {
            return new StoragrClientLogList(this);
        }

        public IStoragrClientRepository Repository(string repositoryIdOrName)
        {
            return new StoragrClientRepository(this, repositoryIdOrName);
        }

        public IStoragrClientRepositoryList Repositories()
        {
            return new StoragrClientRepositoryList(this);
        }

        public IStoragrClientUser User(string userIdOrName)
        {
            return new StoragrClientUser(this, userIdOrName);
        }

        public IStoragrClientUserList Users()
        {
            return new StoragrClientUserList(this);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}