using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Storagr.Client;

namespace Storagr.UI
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly IStoragrClient _client;
        private readonly ILocalStorageService _localStorage;

        public AuthStateProvider(IStoragrClient client, ILocalStorageService localStorage)
        {
            _client = client;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            var token = await _localStorage.GetItemAsStringAsync(Constants.StorageTokenKey);
            
            if (token is not null && await _client.Authenticate(token))
            {
                identity = new ClaimsIdentity(new[]  
                {  
                    new Claim(ClaimTypes.Name, _client.User.Username),  
                }, "storagr");
            }

            return await Task.FromResult(
                new AuthenticationState(new ClaimsPrincipal(identity))
            );
        }
    }
}