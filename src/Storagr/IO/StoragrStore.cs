using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.IO
{
    public sealed class StoragrStoreOptions : StoragrOptions<StoragrStoreOptions>
    {
        public string Host { get; set; }
    }
    
    public sealed class StoragrStore : IStoreAdapter
    {
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly StoragerMediaType _mediaType;
        private readonly StoragrStoreOptions _options;

        public StoragrStore(IOptions<StoragrStoreOptions> optionsAccessor, IUserService userService, IHttpClientFactory clientFactory)
        {
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(StoragrStoreOptions));
            _userService = userService;
            _clientFactory = clientFactory;
            _mediaType = new StoragerMediaType();
        }

        private HttpClient CreateClient(string token)
        {
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri($"https://{_options.Host}");
            client.DefaultRequestHeaders.Add("Accept", $"{_mediaType.MediaType.Value}; charset=utf-8"); // application/vnd.git-lfs+json
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            return client;
        }

        private HttpRequestMessage CreateRequest(string uri) =>
            CreateRequest(uri, HttpMethod.Get);
        private HttpRequestMessage CreateRequest(string uri, HttpMethod method) =>
            new HttpRequestMessage(method, uri);

        private HttpRequestMessage CreateRequest<T>(string uri, HttpMethod method, T data)
        {
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new ByteArrayContent(
                    StoragrHelper.SerializeObject(data)
                )
            };
            request.Content.Headers.Add("Content-Type", $"{_mediaType.MediaType.Value}; charset=utf-8");

            return request;
        }

        public async Task<bool> Verify(string repositoryId, string objectId, long expectedSize)
        {
            var token = await _userService.GetAuthenticatedUserToken();
            var request = CreateRequest($"/{repositoryId}/transfer/{objectId}", HttpMethod.Post, new StoreObject()
            {
                RepositoryId = repositoryId, ObjectId = objectId, Size = expectedSize
            });
            
            return (await CreateClient(token).SendAsync(request)).IsSuccessStatusCode;
        }

        public async Task<StoreRepository> Get(string repositoryId)
        {
            var token = await _userService.GetAuthenticatedUserToken();
            var client = CreateClient(token);
            var request = CreateRequest($"/{repositoryId}");
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync();
            return StoragrHelper.DeserializeObject<StoreRepository>(data);

        }
        public async Task<StoreObject> Get(string repositoryId, string objectId)
        {
            var token = await _userService.GetAuthenticatedUserToken();
            var client = CreateClient(token);
            var request = CreateRequest($"/{repositoryId}/objects/{objectId}");
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync();
            return StoragrHelper.DeserializeObject<StoreObject>(data);
        }

        public async Task<IEnumerable<StoreRepository>> GetAll()
        {
            var token = await _userService.GetAuthenticatedUserToken();
            var client = CreateClient(token);
            var request = CreateRequest($"/");
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync();
            return StoragrHelper.DeserializeObject<IEnumerable<StoreRepository>>(data);
        }
        public async Task<IEnumerable<StoreObject>> GetAll(string repositoryId)
        {
            var token = await _userService.GetAuthenticatedUserToken();
            var client = CreateClient(token);
            var request = CreateRequest($"/{repositoryId}/objects");
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync();
            return StoragrHelper.DeserializeObject<IEnumerable<StoreObject>>(data);
        }

        public async Task Delete(string repositoryId)
        {
            var token = await _userService.GetAuthenticatedUserToken();
            var client = CreateClient(token);
            var request = CreateRequest($"/{repositoryId}", HttpMethod.Delete);

            await client.SendAsync(request);
        }

        public async Task Delete(string repositoryId, string objectId)
        {
            var token = await _userService.GetAuthenticatedUserToken();
            var client = CreateClient(token);
            var request = CreateRequest($"/{repositoryId}/objects/{objectId}", HttpMethod.Delete);
            
            await client.SendAsync(request);
        }

        public async Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId)
        {
            var obj = await Get(repositoryId, objectId);
            if (obj == null)
                return null;
            
            var token = await _userService.GetAuthenticatedUserToken();
            
            return new StoragrAction
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 3600, // 1 hour
                Href = $"{repositoryId}/transfer/{objectId}"
            };
        }

        public async Task<StoragrAction> NewUploadAction(string repositoryId, string objectId)
        {
            var obj = await Get(repositoryId, objectId);
            if (obj != null) 
                return null;
            
            var token = await _userService.GetAuthenticatedUserToken();
            return new StoragrAction
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 3600, // 1 hour
                Href = $"{repositoryId}/transfer/{objectId}"
            };
        }
    }
}