using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public class StoragrClientOptions : StoragrOptions<StoragrClientOptions>
    {
        public string Host { get; set; }
        
        public string Token { get; set; }

        public short DefaultPort { get; set; } = 80;
        public string DefaultProtocol { get; set; } = "https";
    }
    
    public static class StoragrClientService
    {
        public static IServiceCollection AddStoragrClient(this IServiceCollection services, Action<StoragrClientOptions> configureOptions)
        {
            services
                .AddOptions()
                .Configure(configureOptions)
                .AddHttpClient()
                .AddScoped<IStoragrClient, StoragrClient>();
            
            return services;
        }
    }
    
    public partial class StoragrClient : IStoragrClient
    {
        public string Protocol { get; }
        public string Hostname { get; }
        public short Port { get; }
        
        public bool IsAuthenticated { get; private set; }
        public string Token { get; private set; }
        public StoragrUser User { get; private set; }

        private readonly StoragrClientOptions _options;
        private readonly StoragrMediaType _mediaType;
        private readonly HttpClient _httpClient;

        public StoragrClient(IOptions<StoragrClientOptions> optionsAccessor, IHttpClientFactory clientFactory)
        {
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(StoragrClientOptions));
            _mediaType = new StoragrMediaType();

            (Protocol, Hostname, Port) = StoragrHelper.ParseHostname(_options.Host);

            _httpClient = clientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri($"{(Protocol ?? _options.DefaultProtocol)}://{Hostname}:{(Port > 0 ? Port : _options.DefaultPort)}/v1/");
            _httpClient.DefaultRequestHeaders.Add("Accept", $"{_mediaType.MediaType.Value}; charset=utf-8"); // application/vnd.git-lfs+json

            Token = _options.Token;

            if (Token is not null)
                IsAuthenticated = true;
            
            
        }

        private HttpRequestMessage CreateRequest(string uri, HttpMethod method)
        {
            var request = new HttpRequestMessage(method, uri);
            if (IsAuthenticated)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
            return request;
        }
        private HttpRequestMessage CreateRequest<T>(string uri, HttpMethod method, T data)
        {
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new ByteArrayContent(
                    StoragrHelper.SerializeObject(data)
                )
            };
            if (IsAuthenticated)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
            request.Content.Headers.Add("Content-Type", $"{_mediaType.MediaType.Value}; charset=utf-8");

            return request;
        }
        
        public async Task<bool> Authenticate(string token, CancellationToken cancellationToken)
        {
            var response = default(HttpResponseMessage);
            var request = new HttpRequestMessage(HttpMethod.Get, "users/me");
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            try
            {
                response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
            }
            catch
            {
                return false;
            }
            if (!response.IsSuccessStatusCode)
                return false;
            
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var user = StoragrHelper.DeserializeObject<StoragrUser>(content);

            User = user;
            Token = token;
            IsAuthenticated = true;
            
            return true;
        }

        public async Task Authenticate(string username, string password, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"users/authenticate", HttpMethod.Post, new StoragrAuthenticationRequest()
            {
                Username = username,
                Password = password
            });

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                throw (StoragrError) data;
            
            var authenticationResponse = StoragrHelper.DeserializeObject<StoragrAuthenticationResponse>(data);

            Token = authenticationResponse.Token;
            IsAuthenticated = true;
        }

        public IUserCreator CreateUser(string username) =>
            new UserCreator(username, this, _httpClient);

        public IUserUpdater UpdateUser(string userId) =>
            new UserUpdater(userId, this, _httpClient);

        public async Task<StoragrUser> GetUser(string userId, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"users/{userId}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return data;
        }

        public async Task<StoragrUserList> GetUsers(StoragrUserListArgs listArgs, CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(listArgs);
            var request = CreateRequest($"users?{query}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return data;
        }

        public async Task DeleteUser(string userId, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"users/{userId}", HttpMethod.Delete);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw (StoragrError) data;
        }

        public async Task<StoragrLogList> GetLogs(StoragrLogQuery options, CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(options);
            var request = CreateRequest($"logs?{query}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return data;
        }

        public IRepositoryCreator CreateRepository(string name) =>
            new RepositoryCreator(name, this, _httpClient);

        public IRepositoryUpdater UpdateRepository(string repositoryId) =>
            new RepositoryUpdater(repositoryId, this, _httpClient);

        public async Task<StoragrRepository> GetRepository(string repositoryId, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"repositories/{repositoryId}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return data;
        }

        public async Task<StoragrRepositoryList> GetRepositories(StoragrRepositoryListArgs listArgs, CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(listArgs);
            var request = CreateRequest($"repositories?{query}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return data;
        }

        public async Task DeleteRepository(string repositoryId, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"repositories/{repositoryId}", HttpMethod.Delete);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                throw (StoragrError) data;
        }

        public async Task<StoragrBatchObject> BatchObject(string repositoryId, StoragrBatchOperation operation, StoragrObject obj, CancellationToken cancellationToken) =>
            (await BatchObjects(repositoryId, operation, new[] {obj}, cancellationToken)).First();

        public async Task<IEnumerable<StoragrBatchObject>> BatchObjects(string repositoryId, StoragrBatchOperation operation, IEnumerable<StoragrObject> objList, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"{repositoryId}/objects/batch", HttpMethod.Post, new StoragrBatchRequest()
            {
                Operation = operation,
                Transfers = new []{"basic"}, // TODO
                Ref = default, // TODO
                Objects = objList
            });
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return ((StoragrBatchResponse) data).Objects;
        }

        public async Task<StoragrObject> GetObject(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"{repositoryId}/objects/{objectId}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return data;
        }

        public async Task<StoragrObjectList> GetObjects(string repositoryId, StoragrObjectListQuery listQuery, CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(listQuery);
            var request = CreateRequest($"{repositoryId}/objects?{query}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return data;
        }

        public async Task DeleteObject(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"{repositoryId}/objects/{objectId}", HttpMethod.Delete);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                throw (StoragrError) data;
        }

        public async Task<StoragrLock> Lock(string repositoryId, string path, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"{repositoryId}/locks", HttpMethod.Post, new StoragrLockRequest()
            {
                Path = path,
                Ref = default // TODO
            });
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return ((StoragrLockResponse) data).Lock;
        }

        public async Task<StoragrLock> Unlock(string repositoryId, string lockId, bool force, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"{repositoryId}/locks/{lockId}/unlock", HttpMethod.Post, new StoragrUnlockRequest()
            {
               Force = force,
               Ref = default // TODO
            });
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode) 
                throw (StoragrError) data;

            return ((StoragrUnlockResponse) data).Lock;
        }

        public async Task<StoragrLock> GetLock(string repositoryId, string lockId, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"{repositoryId}/locks/{lockId}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw (StoragrError) data;

            return data;
        }

        public async Task<StoragrLockList> GetLocks(string repositoryId, StoragrLockListArgs listArgs, CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(listArgs);
            var request = CreateRequest($"{repositoryId}/locks?{query}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                throw (StoragrError) data;

            return data;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    internal abstract class StoragrClientHelper : IDisposable
    {
        protected StoragrClient StoragrClient { get; private set; }
        protected HttpClient HttpClient { get; private set; }

        protected StoragrClientHelper(StoragrClient storagrClient, HttpClient httpClient)
        {
            StoragrClient = storagrClient;
            HttpClient = httpClient;
        }

        public void Dispose()
        {
            StoragrClient = null;
            HttpClient = null;
        }
    }
}