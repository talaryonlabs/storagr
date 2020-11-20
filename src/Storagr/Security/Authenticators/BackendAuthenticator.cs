using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Storagr.Data;
using Storagr.Data.Entities;

namespace Storagr.Security.Authenticators
{
    public static class BackendAuthenticatorExtension
    {
        public static IServiceCollection AddBackendAuthenticator(this IServiceCollection services)
        {
            return services
                .AddSingleton<BackendAuthenticator>()
                .AddSingleton<IAuthenticationAdapter>(x => x.GetRequiredService<BackendAuthenticator>());
        }
    }
    
    public class BackendAuthenticator : IAuthenticationAdapter
    {
        [Table("_backendAuth")]
        private class Entity
        {
            [ExplicitKey] public string AuthId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Mail { get; set; }
            public string Role { get; set; }
        }
        
        public string Name => "storagr-backend";

        private readonly IBackendAdapter _backend;

        public BackendAuthenticator(IBackendAdapter backend)
        {
            _backend = backend;
        }

        public async Task<AuthenticationResult> Authenticate([NotNull] AuthenticationRequest authenticationRequest)
        {
            var backendUser = await _backend.Get<Entity>(q => q.Where(f => f.Equal("Username", authenticationRequest.Username)));
            if (backendUser == null)
                return null;
            
            var hasher = new PasswordHasher<Entity>();
            if(hasher.VerifyHashedPassword(backendUser, backendUser.Password, authenticationRequest.Password) != PasswordVerificationResult.Success)
                return null;

            return new AuthenticationResult()
            {
                Values =
                {
                    {AuthenticationResultType.Id, backendUser.AuthId},
                    {AuthenticationResultType.Username, backendUser.Username},
                    {AuthenticationResultType.Mail, backendUser.Mail},
                    {AuthenticationResultType.Role, backendUser.Role}
                }
            };
        }

        public async Task Create([NotNull] string username, [NotNull] string password, [AllowNull] string mail, [AllowNull] string role)
        {
            var uuid = StoragrHelper.UUID();
            var hasher = new PasswordHasher<Entity>();
            var entity = new Entity()
            {
                AuthId = uuid,
                Username = username,
                Password = hasher.HashPassword(null, password),
                Mail = mail ?? "",
                Role = role ?? ""
            };
            await _backend.Insert(entity);
        }

        public async Task Modify([NotNull] string authId, [AllowNull] string username, [AllowNull] string password,
            [AllowNull] string mail, [AllowNull] string role)
        {
            var user = await _backend.Get<Entity>(q => q.Where(f => f.Equal("AuthId", authId)));
            var hasher = new PasswordHasher<Entity>();
            var newPassword = (password != null ? hasher.HashPassword(null, password) : null);
            var entity = new Entity()
            {
                AuthId = user.AuthId,
                Username = username ?? user.Username,
                Password = newPassword ?? user.Password,
                Mail = mail ?? user.Mail,
                Role = role ?? user.Role
            };
            await _backend.Update(entity);
        }

        public async Task Delete([NotNull] string authId)
        {
            await _backend.Delete(new Entity() {AuthId = authId});
        }
    }
}