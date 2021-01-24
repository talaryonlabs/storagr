using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Data;

namespace Storagr.Client
{
    internal class StoragrClientAuthenticator : 
        StoragrClientHelper<bool>, 
        IStoragrClientAuthenticator
    {
        private readonly IStoragrClient _client;
        private StoragrAuthenticationRequest _userdata;
        private string _token;
        
        public StoragrClientAuthenticator(IStoragrClient client, IStoragrClientRequest clientRequest) 
            : base(clientRequest)
        {
            _client = client;
        }

        protected override async Task<bool> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            if (_token is not null)
            {
                // TODO                
            }

            if (_userdata is not null)
            {
                var request = await clientRequest.Send<StoragrAuthenticationResponse, StoragrAuthenticationRequest>(
                    $"users/authenticate",
                    HttpMethod.Post,
                    _userdata,
                    cancellationToken
                );
                _token = request.Token;
            }

            _client.UseToken(_token);

            return false;
        }

        IStoragrRunner<bool> IStoragrClientAuthenticator.With(string username, string password)
        {
            _userdata = new StoragrAuthenticationRequest()
            {
                Username = username,
                Password = password
            };
            return this;
        }

        IStoragrRunner<bool> IStoragrClientAuthenticator.With(string token)
        {
            _token = token;
            return this;
        }
    }
}