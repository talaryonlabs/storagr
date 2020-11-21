using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Storagr.Services;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.IO
{
    public sealed class StoragrStore : IStoreAdapter
    {
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly StoragerMediaTypeHeader _mediaType;

        public StoragrStore(IUserService userService, IHttpClientFactory clientFactory)
        {
            _userService = userService;
            _clientFactory = clientFactory;
            _mediaType = new StoragerMediaTypeHeader();
        }

        private async Task<HttpRequestMessage> CreateMessage(HttpMethod method, string uri)
        {
            var token = await _userService.GetAuthenticatedUserToken();
            var request = new HttpRequestMessage(method, uri);
            
            request.Headers.Add("Content-Type", $"{_mediaType.MediaType.Value}; charset=utf-8");
            request.Headers.Add("Accept", $"{_mediaType.MediaType.Value}; charset=utf-8"); // application/vnd.git-lfs+json
            request.Headers.Add("Authorization", $"Bearer {token}");

            return request;
        }

        public async Task<StoreRepository> Get(string repositoryId)
        {
            var client = _clientFactory.CreateClient();
            var request = await CreateMessage(HttpMethod.Get, $"/objects/{repositoryId}/info");
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync();
            return StoragrHelper.DeserializeObject<StoreRepository>(data);

        }
        public async Task<StoreObject> Get(string repositoryId, string objectId)
        {
            var client = _clientFactory.CreateClient();
            var request = await CreateMessage(HttpMethod.Get, $"/objects/{repositoryId}/{objectId}");
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync();
            return StoragrHelper.DeserializeObject<StoreObject>(data);
        }

        public async Task<IEnumerable<StoreRepository>> GetAll()
        {
            var client = _clientFactory.CreateClient();
            var request = await CreateMessage(HttpMethod.Get, $"/objects");
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync();
            return StoragrHelper.DeserializeObject<IEnumerable<StoreRepository>>(data);
        }
        public async Task<IEnumerable<StoreObject>> GetAll(string repositoryId)
        {
            var client = _clientFactory.CreateClient();
            var request = await CreateMessage(HttpMethod.Get,  $"/objects/{repositoryId}");
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync();
            return StoragrHelper.DeserializeObject<IEnumerable<StoreObject>>(data);
        }

        public async Task Delete(string repositoryId)
        {
            var client = _clientFactory.CreateClient();
            var request = await CreateMessage(HttpMethod.Delete, $"/objects/{repositoryId}");
            
            await client.SendAsync(request);
        }
        public async Task Delete(string repositoryId, string objectId)
        {
            var client = _clientFactory.CreateClient();
            var request = await CreateMessage(HttpMethod.Delete, $"/objects/{repositoryId}/{objectId}");
            
            await client.SendAsync(request);
        }

        public async Task<StoragrAction> NewDownloadRequest(string repositoryId, string objectId)
        {
            var obj = await Get(repositoryId, objectId);
            if (obj == null)
                return null;
            
            var token = await _userService.GetAuthenticatedUserToken();
            
            return new StoragrAction
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 0,
                Href = $"{repositoryId}/store/{objectId}"
            };
        }

        public async Task<StoragrAction> NewUploadRequest(string repositoryId, string objectId)
        {
            var obj = await Get(repositoryId, objectId);
            if (obj != null) 
                return null;
            
            var token = await _userService.GetAuthenticatedUserToken();
            return new StoragrAction
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 0,
                Href = $"{repositoryId}/store/{objectId}"
            };
        }
    }
}