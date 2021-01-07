using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Data.Entities;
using Storagr.Security.Authenticators;
using Storagr.Security.Tokens;
using Storagr.Shared;

namespace Storagr.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationAdapter _authenticator;
        private readonly IDatabaseAdapter _database;
        private readonly IDistributedCache _cache;
        private readonly ITokenService _tokenService;
        private readonly DistributedCacheEntryOptions _cacheEntryOptions;

        public UserService(IDatabaseAdapter database, ITokenService tokenService, IDistributedCache cache, IAuthenticationAdapter authenticator, IHttpContextAccessor httpContextAccessor)
        {
            _database = database;
            _tokenService = tokenService;
            _cache = cache;
            _authenticator = authenticator;
            _httpContextAccessor = httpContextAccessor;
            _cacheEntryOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // TODO time from a global config
        }

        public async Task<UserEntity> Authenticate(string username, string password, CancellationToken cancellationToken)
        {
            var result = await _authenticator.Authenticate(username, password, cancellationToken);
            if (result is null)
                return null;

            var user = await _database.Get<UserEntity>(filter =>
            {
                filter
                    .Equal(nameof(UserEntity.AuthAdapter), _authenticator.Name)
                    .And()
                    .Equal(nameof(UserEntity.AuthId), result.Id);
            }, cancellationToken);
            if (user is null)
            {
                await _database.Insert(user = new UserEntity()
                {
                    Id = StoragrHelper.UUID(),
                    IsEnabled = true,
                    IsAdmin = !(await _database.GetAll<UserEntity>(cancellationToken)).Any(), // first created user is admin
                    AuthAdapter = _authenticator.Name,
                    AuthId = result.Id,
                    Username = result.Username,
                }, cancellationToken);
            }
            else if(user.Username != result.Username)
            {
                user.Username = result.Username;
                await _database.Update(user, cancellationToken);
            }
            
            var token = new UserToken()
            {
                UserId = user.Id,
                Role = user.IsAdmin ? StoragrConstants.ManagementRole : default
            };
            user.Token = _tokenService.Generate(token) ?? throw new Exception("Unable to generate token!");

            return user;
        }

        public async Task<int> Count(string username, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetUserCountKey();
            var useCache = (username is null);
            if (useCache && await _cache.ExistsAsync(key, cancellationToken))
            {
                return await _cache.GetObjectAsync<int>(key, cancellationToken);
            }
            var count = await _database.Count<UserEntity>(filter =>
            {
                if(username is not null)
                    filter.In(nameof(UserEntity.Username), new[] {username});
            }, cancellationToken);

            if (useCache)
            {
                await _cache.SetObjectAsync(key, count, _cacheEntryOptions, cancellationToken);
            }

            return count;
        }
            

        public async Task<bool> Exists(string userIdOrName, CancellationToken cancellationToken = default) =>
            await _cache.ExistsAsync(StoragrCaching.GetUserKey(userIdOrName), cancellationToken)
            ||
            await _database.Exists<UserEntity>(filter =>
            {
                filter
                    .Equal(nameof(UserEntity.Id), userIdOrName)
                    .Or()
                    .Equal(nameof(UserEntity.Username), userIdOrName);
            }, cancellationToken);

        public async Task<UserEntity> Get(string userIdOrName, CancellationToken cancellationToken)
        {
            var key = StoragrCaching.GetUserKey(userIdOrName);
            var user =
                await _cache.GetObjectAsync<UserEntity>(key, cancellationToken) ??
                await _database.Get<UserEntity>(filter =>
                {
                    filter
                        .Equal(nameof(UserEntity.Id), userIdOrName)
                        .Or()
                        .Equal(nameof(UserEntity.Username), userIdOrName);
                }, cancellationToken) ??
                throw new UserNotFoundError();

            await _cache.SetObjectAsync(key, user, _cacheEntryOptions, cancellationToken);
            return user;
        }

        public Task<IEnumerable<UserEntity>> GetMany(string username = null, CancellationToken cancellationToken = default) =>
            _database.GetMany<UserEntity>(filter =>
            {
                if(username is not null)
                    filter.In(nameof(UserEntity.Username), new[] {username});
            }, cancellationToken);


        public Task<IEnumerable<UserEntity>> GetAll(CancellationToken cancellationToken) =>
            _database.GetAll<UserEntity>(cancellationToken);

        /*public async Task<UserEntity> Create(string username, string password, bool isAdmin)
        {
            if (!(_authenticator is BackendAuthenticator authenticator))
                throw new NotImplementedError();
            if(await ExistsByUsername(username))
                throw new UserAlreadyExistsError(null); // TODO

            var entity = default(UserEntity);
            var result = await authenticator.Create(username, password);

            await _database.Insert(entity = new UserEntity()
            {
                Id = StoragrHelper.UUID(),
                IsEnabled = true,
                IsAdmin = isAdmin,
                AuthAdapter = _authenticator.Name,
                AuthId = result.Id,
                Username = result.Username,
            });

            return entity;
        }*/
        
        public async Task<UserEntity> Create(UserEntity entity, CancellationToken cancellationToken)
        {
            var existingUser = await Get(entity.Id, cancellationToken);
            if(existingUser is not null)
                throw new UserAlreadyExistsError(existingUser);

            entity.Id = StoragrHelper.UUID();

            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetUserCountKey(), cancellationToken),
                _database.Insert(entity, cancellationToken)
            );

            return entity;
        }

        public async Task<UserEntity> Modify(UserEntity entity, CancellationToken cancellationToken)
        {
            var user = await Get(entity.Id, cancellationToken) ??
                       throw new UserNotFoundError();

            // TODO
            // if (_authenticator is BackendAuthenticator authenticator)
            // {
            //     await authenticator.Modify(entity.AuthId, entity.Username, newPassword);
            // }

            user.IsAdmin = entity.IsAdmin;
            user.IsEnabled = entity.IsEnabled;
            user.Username = entity.Username;

            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetUserCountKey(), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetUserKey(user.Id), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetUserKey(user.Username), cancellationToken),
                _database.Update(user, cancellationToken)
            );

            return user;
        }


        public Task<UserEntity> GetAuthenticatedUser(CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext ?? throw new NullReferenceException();
            var userId = context.User.FindFirst(StoragrConstants.TokenUnqiueId)?.Value;

            return userId is null
                ? null
                : Get(userId, cancellationToken);
        }

        public async Task<string> GetAuthenticatedToken(CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext ?? throw new NullReferenceException();
            var token = await Task.Run(() => context.GetTokenAsync("Bearer", "access_token"), cancellationToken);
            token ??= await Task.Run(() => context.GetTokenAsync("Basic", "access_token"), cancellationToken);

            return token;
        }

        public async Task<UserEntity> Delete(string userId, CancellationToken cancellationToken)
        {
            var user = await Get(userId, cancellationToken) ??
                       throw new UserNotFoundError();
            
            if (_authenticator is BackendAuthenticator authenticator)
            {
                await authenticator.Delete(user.AuthId, cancellationToken);
            }

            await Task.WhenAll(
                _cache.RemoveAsync(StoragrCaching.GetUserCountKey(), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetUserKey(user.Id), cancellationToken),
                _cache.RemoveAsync(StoragrCaching.GetUserKey(user.Username), cancellationToken),
                _database.Delete(user, cancellationToken)
            );

            return user;
        }
    }
}