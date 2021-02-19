using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Storagr.Server.Security.Tokens;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server.IO
{
    [StoragrConfig("StoragrStore")]
    public sealed class StoragrStoreOptions : StoragrOptions<StoragrStoreOptions>
    {
        [StoragrConfigValue] public string Host { get; set; }

        [StoragrConfigValue(IsNamedDelay = true)]
        public TimeSpan RequestExpiration { get; set; }

        [StoragrConfigValue(IsNamedDelay = true)]
        public TimeSpan TransferExpiration { get; set; }
    }

    public sealed class StoragrStore2 :
        IStoreAdapter,
        IStoragrRunner<StoreInformation>
    {
        private readonly StoragrStoreOptions _options;
        private readonly ITokenService _tokenService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly StoragrMediaType _mediaType;
        private readonly StoreToken _token;


        public StoragrStore2(IOptions<StoragrStoreOptions> optionsAccessor, ITokenService tokenService,
            IHttpClientFactory clientFactory)
        {
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(StoragrStoreOptions));
            _tokenService = tokenService;
            _clientFactory = clientFactory;
            _mediaType = new StoragrMediaType();
            _token = new StoreToken() {UniqueId = "storagr-api"};
        }

        private string GenerateToken() => _tokenService.Generate(_token, _options.RequestExpiration);
        private string GenerateTransferToken() => _tokenService.Generate(_token, _options.TransferExpiration);

        private HttpClient CreateClient()
        {
            var token = GenerateToken();
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri($"https://{_options.Host}/v1/");
            client.DefaultRequestHeaders.Add("Accept",
                $"{_mediaType.MediaType.Value}; charset=utf-8"); // application/vnd.git-lfs+json
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            return client;
        }

        private async Task<HttpResponseMessage> Head(string uri, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Head, uri);
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return response;
        }

        private async Task<TResponse> Send<TResponse>(string uri, HttpMethod method, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            var request = new HttpRequestMessage(method, uri);
            var response = await client.SendAsync(request, cancellationToken);
            var responseData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw StoragrHelper.DeserializeObject<StoragrError>(responseData);

            return StoragrHelper.DeserializeObject<TResponse>(responseData);
        }

        private async Task<TResponse> Send<TResponse, TRequestData>(string uri, HttpMethod method, TRequestData requestData,
            CancellationToken cancellationToken)
        {
            var client = CreateClient();
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new ByteArrayContent(
                    StoragrHelper.SerializeObject(requestData)
                )
            };
            request.Content.Headers.Add("Content-Type",
                $"{_mediaType.MediaType.Value}; charset=utf-8"); // application/vnd.git-lfs+json

            var response = await client.SendAsync(request, cancellationToken);
            var responseData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw StoragrHelper.DeserializeObject<StoragrError>(responseData);

            return StoragrHelper.DeserializeObject<TResponse>(responseData);
        }


        public IStoragrRunner<StoreInformation> Info() => this;

        StoreInformation IStoragrRunner<StoreInformation>.Run() => (this as IStoragrRunner<StoreInformation>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        async Task<StoreInformation> IStoragrRunner<StoreInformation>.RunAsync(CancellationToken cancellationToken) =>
            await Send<StoreInformation>("info", HttpMethod.Get, cancellationToken);


        public IStoreAdapterObject Object(string objectId) => 
            new StoragrStoreObject(this, objectId);

        private class StoragrStoreObject :
            IStoreAdapterObject,
            IStoragrRunner,
            IStoragrRunner<bool>,
            IStoragrRunner<StoragrAction>
        {
            private readonly StoragrStore2 _store;
            private readonly string _objectId;
            private bool _downloadRequest;
            private bool _uploadRequest;

            public StoragrStoreObject(StoragrStore2 store, string objectId)
            {
                _store = store;
                _objectId = objectId;
            }

            public IStoragrRunner<bool> Exists() => this;

            bool IStoragrRunner<bool>.Run() => (this as IStoragrRunner<bool>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<bool> IStoragrRunner<bool>.RunAsync(CancellationToken cancellationToken)
            {
                var response = await _store.Head($"objects/{_objectId}", cancellationToken);

                return !response.IsSuccessStatusCode;
            }

            StoreObject IStoragrRunner<StoreObject>.Run() => (this as IStoragrRunner<StoreObject>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<StoreObject> IStoragrRunner<StoreObject>.RunAsync(CancellationToken cancellationToken)
            {
                var response = await _store.Head($"objects/{_objectId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw response.StatusCode switch
                    {
                        HttpStatusCode.NotFound => new ObjectNotFoundError(),
                        _ => new InternalServerError()
                    };

                var contentLength = response.Headers.GetValues("Content-Length").First();
                return new StoreObject()
                {
                    ObjectId = _objectId,
                    Size = long.Parse(contentLength)
                };
            }

            public IStoragrRunner Delete(bool force = false) => this;
            void IStoragrRunner.Run() => (this as IStoragrRunner)
                .RunAsync()
                .RunSynchronously();

            async Task IStoragrRunner.RunAsync(CancellationToken cancellationToken)
            {
                var client = _store.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"objects/{_objectId}");
                var response = await client.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw StoragrHelper.DeserializeObject<StoragrError>(
                        await response.Content.ReadAsByteArrayAsync(cancellationToken)
                    );
            }

            public IStoragrRunner<StoragrAction> Download()
            {
                _downloadRequest = true;
                return this;
            }

            public IStoragrRunner<StoragrAction> Upload()
            {
                _uploadRequest = true;
                return this;
            }

            StoragrAction IStoragrRunner<StoragrAction>.Run() => (this as IStoragrRunner<StoragrAction>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            Task<StoragrAction> IStoragrRunner<StoragrAction>.RunAsync(CancellationToken cancellationToken)
            {
                var token = _store.GenerateTransferToken();
                var action = new StoragrAction()
                {
                    Href = $"objects/{_objectId}",
                    Header = new Dictionary<string, string>()
                    {
                        {"Authorization", $"Bearer {token}"}
                    },
                    ExpiresIn = (int)_store._options.TransferExpiration.TotalSeconds
                };
                
                return Task.FromResult(action);
            }

            
        }
    }
}