using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Storagr.Server.Data.Entities;

namespace Storagr.Server.Security.Authenticators
{
    public class DefaultAuthenticator : IAuthenticationAdapter
    {
        public string Name => "default";

        private readonly IDatabaseAdapter _database;

        public DefaultAuthenticator(IDatabaseAdapter database)
        {
            _database = database;
        }

        public Task<IAuthenticationResult> Authenticate(string token, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task<IAuthenticationResult> Authenticate(string username, string password, CancellationToken cancellationToken)
        {
            var user = await _database
                .First<UserEntity>()
                .Where(filter => filter
                    .Is(nameof(UserEntity.Username))
                    .EqualTo(username)
                )
                .RunAsync(cancellationToken);
            
            if (user is null)
                return null;

            return new PasswordHasher<UserEntity>()
                .VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Success
                ? new AuthenticationResult()
                {
                    Id = user.Id,
                    Username = user.Username
                }
                : null;
        }

        private class AuthenticationResult :
            IAuthenticationResult
        {
            public string Id { get; set; }
            public string Username { get; set; }
        }
    }
}