using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Storagr.Server.Data.Entities;
using Storagr.Server.Security.Tokens;
using Storagr.Shared;

namespace Storagr.Server.Services
{
    public partial class StoragrService
    {
        private class Authentication :
            IStoragrServiceAuthorization,
            IStoragrServiceAuthorizationAuthentication,
            IStoragrRunner<IStoragrServiceAuthorizationResult>,
            IStoragrRunner<UserEntity>,
            IStoragrRunner<string>
        {
            private readonly StoragrService _storagrService;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private string[] _credentials;
            private string _token;

            public Authentication(StoragrService storagrService, IHttpContextAccessor httpContextAccessor)
            {
                _storagrService = storagrService;
                _httpContextAccessor = httpContextAccessor;
            }

            /**
             * Authentication
             */
            IStoragrServiceAuthorizationAuthentication IStoragrServiceAuthorization.Authenticate() => this;

            IStoragrRunner<IStoragrServiceAuthorizationResult> IStoragrServiceAuthorizationAuthentication.With(string username, string password)
            {
                _credentials = new[] {username, password};
                return this;
            }

            IStoragrRunner<IStoragrServiceAuthorizationResult> IStoragrServiceAuthorizationAuthentication.With(string token)
            {
                _token = token;
                return this;
            }

            IStoragrServiceAuthorizationResult IStoragrRunner<IStoragrServiceAuthorizationResult>.Run() =>
                (this as IStoragrRunner<IStoragrServiceAuthorizationResult>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<IStoragrServiceAuthorizationResult> IStoragrRunner<IStoragrServiceAuthorizationResult>.RunAsync(
                CancellationToken cancellationToken)
            {
                // TODO throw exception on failure

                var result = await _storagrService.Authenticator.Authenticate(_credentials[0], _credentials[1], cancellationToken);
                if (result is null)
                {
                    throw new UnauthorizedError("Invalid username and/or password.");
                }

                var authName = _storagrService.Authenticator.Name;
                var user = await _storagrService
                    .Database
                    .First<UserEntity>()
                    .Where(filter => filter
                        .Is(nameof(UserEntity.AuthAdapter))
                        .EqualTo(authName)
                        .And()
                        .Is(nameof(UserEntity.AuthId))
                        .EqualTo(result.Id)
                    )
                    .RunAsync(cancellationToken);

                if (user is null)
                {
                    var count = await _storagrService
                        .Database
                        .Count<UserEntity>()
                        .RunAsync(cancellationToken);

                    await _storagrService
                        .Database
                        .Insert(user = new UserEntity()
                        {
                            Id = StoragrHelper.UUID(),
                            IsEnabled = true,
                            IsAdmin = (count == 0), // first created user is admin
                            AuthAdapter = authName,
                            AuthId = result.Id,
                            Username = result.Username,
                        })
                        .RunAsync(cancellationToken);
                }
                else if (user.Username != result.Username)
                {
                    user.Username = result.Username;
                    await _storagrService
                        .Database
                        .Update(user)
                        .RunAsync(cancellationToken);
                }

                var token = new UserToken()
                {
                    UserId = user.Id,
                    Role = user.IsAdmin ? StoragrConstants.ManagementRole : default
                };
                user.Token = _storagrService
                                 .TokenService
                                 .Generate(token)
                             ?? throw new Exception("Unable to generate token!");

                return new AuthenticationResult(user, user.Token);
            }

            /**
             * GetAuthenticatedUser
             */
            IStoragrRunner<UserEntity> IStoragrServiceAuthorization.GetAuthenticatedUser() => this;

            UserEntity IStoragrRunner<UserEntity>.Run() => (this as IStoragrRunner<UserEntity>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            Task<UserEntity> IStoragrRunner<UserEntity>.RunAsync(CancellationToken cancellationToken)
            {
                var context = _httpContextAccessor.HttpContext ?? throw new NullReferenceException();
                var userId = context.User.FindFirst(StoragrConstants.TokenUnqiueId)?.Value;

                return (_storagrService as IStoragrService)
                    .User(userId)
                    .RunAsync(cancellationToken);
            }

            /**
             * GetAuthenticatedToken
             */
            IStoragrRunner<string> IStoragrServiceAuthorization.GetAuthenticatedToken() => this;

            string IStoragrRunner<string>.Run() => (this as IStoragrRunner<string>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<string> IStoragrRunner<string>.RunAsync(CancellationToken cancellationToken)
            {
                var context = _httpContextAccessor.HttpContext ?? throw new NullReferenceException();
                var token = await Task.Run(() => context.GetTokenAsync("Bearer", "access_token"), cancellationToken);
                token ??= await Task.Run(() => context.GetTokenAsync("Basic", "access_token"), cancellationToken);

                return token;
            }


        }

        private class AuthenticationResult : IStoragrServiceAuthorizationResult
        {
            public AuthenticationResult(UserEntity user, string token)
            {
                User = user;
                Token = token;
            }

            public UserEntity User { get; }
            public string Token { get; }
        }
    }
}