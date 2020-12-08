using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public class StoragrClientOptions : StoragrOptions<StoragrClientOptions>
    {
        public string Host { get; set; }
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
    
    public class StoragrClient : IStoragrClient
    {
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
            
            _httpClient = clientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri($"http://{_options.Host}");
            _httpClient.DefaultRequestHeaders.Add("Accept", $"{_mediaType.MediaType.Value}; charset=utf-8"); // application/vnd.git-lfs+json
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
        
        public async Task<bool> Authenticate(string token)
        {
            var response = default(HttpResponseMessage);
            var request = new HttpRequestMessage(HttpMethod.Get, "users/me");
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            try
            {
                response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            }
            catch
            {
                return false;
            }
            if (!response.IsSuccessStatusCode)
                return false;
            
            var content = await response.Content.ReadAsByteArrayAsync();
            var user = StoragrHelper.DeserializeObject<StoragrUser>(content);

            User = user;
            Token = token;
            IsAuthenticated = true;
            
            return true;
        }

        public async Task<bool> Authenticate(string username, string password)
        {
            var response = default(HttpResponseMessage);
            var request = CreateRequest("users/authenticate", HttpMethod.Post, new StoragrAuthenticationRequest()
            {
                Username = username,
                Password = password
            });
            try
            {
                response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            }
            catch
            {
                return false;
            }
            if (!response.IsSuccessStatusCode)
                return false;
            
            var content = await response.Content.ReadAsByteArrayAsync();
            var authenticationResponse = StoragrHelper.DeserializeObject<StoragrAuthenticationResponse>(content);

            Token = authenticationResponse.Token;
            IsAuthenticated = true;

            return true;
        }

        public async Task<StoragrUser> GetUser(string userId)
        {
            var request = CreateRequest($"/users/{userId}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();
            
            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return data;
        }

        public async Task<IEnumerable<StoragrUser>> GetUsers()
        {
            var request = CreateRequest("/users", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var data = await response.Content.ReadAsByteArrayAsync();

            return StoragrHelper.DeserializeObject<IEnumerable<StoragrUser>>(data);
        }

        public async Task<StoragrLogList> GetLogs(StoragrLogListOptions options)
        {
            var query = StoragrHelper.ToQueryString(options);
            var request = CreateRequest($"/logs?{query}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();
            
            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return data;
        }

        public async Task<StoragrRepository> CreateRepository(string repositoryId, string ownerId, long sizeLimit)
        {
            var request = CreateRequest($"/", HttpMethod.Post, new StoragrRepository()
            {
                RepositoryId = repositoryId,
                OwnerId = ownerId,
                SizeLimit = sizeLimit
            });
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();

            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return data;
        }

        public async Task<StoragrRepository> GetRepository(string repositoryId)
        {
            var request = CreateRequest($"/{repositoryId}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();
            
            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return data;
        }

        public async Task<IEnumerable<StoragrRepository>> GetRepositories()
        {
            var request = CreateRequest("/", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();
            
            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return StoragrHelper.DeserializeObject<IEnumerable<StoragrRepository>>(data);
        }

        public async Task DeleteRepository(string repositoryId)
        {
            var request = CreateRequest($"/{repositoryId}", HttpMethod.Delete);
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(
                    await response.Content.ReadAsByteArrayAsync()
                );
        }

        public async Task<StoragrBatchObject> BatchObject(string repositoryId, StoragrBatchOperation operation, StoragrObject obj) =>
            (await BatchObjects(repositoryId, operation, new[] {obj})).First();

        public async Task<IEnumerable<StoragrBatchObject>> BatchObjects(string repositoryId, StoragrBatchOperation operation, IEnumerable<StoragrObject> objList)
        {
            var request = CreateRequest($"/{repositoryId}/objects/batch", HttpMethod.Post, new StoragrBatchRequest()
            {
                Operation = operation,
                Transfers = new []{"basic"}, // TODO
                Ref = default, // TODO
                Objects = objList
            });
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();
            
            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return ((StoragrBatchResponse) data).Objects;
        }

        public async Task<StoragrObject> GetObject(string repositoryId, string objectId)
        {
            var request = CreateRequest($"/{repositoryId}/objects/{objectId}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();
            
            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return data;
        }

        public async Task<StoragrObjectList> GetObjects(string repositoryId, StoragrObjectListOptions options)
        {
            var query = StoragrHelper.ToQueryString(options);
            var request = CreateRequest($"/{repositoryId}/objects?{query}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();

            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return data;
        }

        public async Task DeleteObject(string repositoryId, string objectId)
        {
            var request = CreateRequest($"/{repositoryId}/objects/{objectId}", HttpMethod.Delete);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new StoragrException(
                    await response.Content.ReadAsByteArrayAsync()
                );
        }

        public async Task<StoragrLock> CreateLock(string repositoryId, string path)
        {
            var request = CreateRequest($"/{repositoryId}/locks", HttpMethod.Post, new StoragrLockRequest()
            {
                Path = path,
                Ref = default // TODO
            });
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();

            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return ((StoragrLockResponse) data).Lock;
        }

        public async Task<StoragrLock> DeleteLock(string repositoryId, string lockId, bool force)
        {
            var request = CreateRequest($"/{repositoryId}/locks/{lockId}/unlock", HttpMethod.Post, new StoragrLockUnlockRequest()
            {
               Force = force,
               Ref = default // TODO
            });
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();

            if (!response.IsSuccessStatusCode) 
                throw new StoragrException(data);

            return ((StoragrLockUnlockResponse) data).Lock;
        }

        public async Task<StoragrLock> GetLock(string repositoryId, string lockId)
        {
            var request = CreateRequest($"/{repositoryId}/locks/{lockId}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();
            
            if (!response.IsSuccessStatusCode)
                throw new StoragrException(data);

            return data;
        }

        public async Task<StoragrLockList> GetLocks(string repositoryId, StoragrLockListOptions options)
        {
            var query = StoragrHelper.ToQueryString(options);
            var request = CreateRequest($"/{repositoryId}/locks?{query}", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();
            
            if (!response.IsSuccessStatusCode)
                throw new StoragrException(data);

            return data;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}