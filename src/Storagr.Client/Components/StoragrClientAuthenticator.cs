using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientAuthenticator : StoragrClientHelper, IStoragrClientAuthenticator, IStoragrClientRunner<bool>
    {
        private readonly IStoragrClient _client;
        private StoragrAuthenticationRequest _userdata;
        private string _token;
        
        public StoragrClientAuthenticator(IStoragrClient client, IStoragrClientRequest clientRequest) 
            : base(clientRequest)
        {
            _client = client;
        }
        
        IStoragrClientRunner<bool> IStoragrClientAuthenticator.With(string username, string password)
        {
            _userdata = new StoragrAuthenticationRequest()
            {
                Username = username,
                Password = password
            };
            return this;
        }

        IStoragrClientRunner<bool> IStoragrClientAuthenticator.With(string token)
        {
            _token = token;
            return this;
        }

        bool IStoragrClientRunner<bool>.Run()
        {
            var task = (this as IStoragrClientRunner<bool>).RunAsync();
            task.RunSynchronously();
            return task.Result;
        }

        async Task<bool> IStoragrClientRunner<bool>.RunAsync(CancellationToken cancellationToken)
        {
            if (_token is not null)
            {
                // TODO                
            }

            if (_userdata is not null)
            {
                var request = await Request<StoragrAuthenticationResponse, StoragrAuthenticationRequest>(
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
    }
}