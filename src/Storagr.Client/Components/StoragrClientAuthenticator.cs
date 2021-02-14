using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientAuthenticator : 
        IStoragrClientAuthenticator,
        IStoragrRunner<bool>
    {
        private readonly IStoragrClient _client;
        private readonly IStoragrClientRequest _clientRequest;
        private StoragrAuthenticationRequest _userdata;
        private string _token;
        
        public StoragrClientAuthenticator(IStoragrClient client, IStoragrClientRequest clientRequest)
        {
            _client = client;
            _clientRequest = clientRequest;
        }
        
        bool IStoragrRunner<bool>.Run() => (this as IStoragrRunner<bool>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        async Task<bool> IStoragrRunner<bool>.RunAsync(CancellationToken cancellationToken)
        {
            if (_token is not null)
            {
                // TODO                
            }

            if (_userdata is not null)
            {
                var request = await _clientRequest.Send<StoragrAuthenticationResponse, StoragrAuthenticationRequest>(
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