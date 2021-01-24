using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Storagr.Server.Security.Tokens;

namespace Storagr.Server.IO
{
    [StoragrConfig("StoragrStore")]
    public sealed class StoragrStoreOptions : StoragrOptions<StoragrStoreOptions>
    {
        [StoragrConfigValue] public string Host { get; set; }
        [StoragrConfigValue(IsNamedDelay = true)] public TimeSpan DefaultExpiration { get; set; }
        [StoragrConfigValue(IsNamedDelay = true)] public TimeSpan TransferExpiration { get; set; }
    }

    public sealed class StoragrStore : IStoreAdapter
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly StoragrMediaType _mediaType;
        private readonly StoragrStoreOptions _options;
        private readonly StoreToken _token;

        public StoragrStore(IOptions<StoragrStoreOptions> optionsAccessor, ITokenService tokenService, IUserService userService, IHttpClientFactory clientFactory)
        {
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(StoragrStoreOptions));
            _tokenService = tokenService;
            _userService = userService;
            _clientFactory = clientFactory;
            _mediaType = new StoragrMediaType();
            _token = new StoreToken() {UniqueId = "storagr-api"};
        }

        private HttpClient CreateClient()
        {
            var token = _tokenService.Generate(_token, _options.DefaultExpiration);
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri($"https://{_options.Host}/v1/");
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

        public async Task<bool> Finalize(string repositoryId, string objectId, ulong expectedSize, CancellationToken cancellationToken)
        {
            var request = CreateRequest($"{repositoryId}/transfer/{objectId}", HttpMethod.Post, new StoreObject()
            {
                RepositoryId = repositoryId, ObjectId = objectId, Size = expectedSize
            });
            
            return (await CreateClient().SendAsync(request, cancellationToken)).IsSuccessStatusCode;
        }

        public async Task<StoreRepository> Get(string repositoryId, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            var request = CreateRequest($"{repositoryId}");
            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return StoragrHelper.DeserializeObject<StoreRepository>(data);

        }
        public async Task<StoreObject> Get(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            var request = CreateRequest($"{repositoryId}/objects/{objectId}");
            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return StoragrHelper.DeserializeObject<StoreObject>(data);
        }

        public async Task<IEnumerable<StoreRepository>> GetAll(CancellationToken cancellationToken)
        {
            var client = CreateClient();
            var request = CreateRequest($"");
            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return StoragrHelper.DeserializeObject<IEnumerable<StoreRepository>>(data);
        }
        public async Task<IEnumerable<StoreObject>> GetAll(string repositoryId, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            var request = CreateRequest($"objects?r={repositoryId}");
            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return StoragrHelper.DeserializeObject<IEnumerable<StoreObject>>(data);
        }

        public async Task Delete(string repositoryId, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            var request = CreateRequest($"{repositoryId}", HttpMethod.Delete);

            await client.SendAsync(request, cancellationToken);
        }

        public async Task Delete(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            var request = CreateRequest($"{repositoryId}/objects/{objectId}", HttpMethod.Delete);
            
            await client.SendAsync(request, cancellationToken);
        }

        public async Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            var obj = await Get(repositoryId, objectId, cancellationToken);
            if (obj is null)
                return null;

            var token = _tokenService.Generate(_token, _options.TransferExpiration);
            
            return new StoragrAction
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 3600, // 1 hour
                Href = $"{repositoryId}/transfer/{objectId}"
            };
        }

        public async Task<StoragrAction> NewUploadAction(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            var obj = await Get(repositoryId, objectId, cancellationToken);
            if (obj is not null) 
                return null;

            var token = _tokenService.Generate(_token, _options.TransferExpiration);
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