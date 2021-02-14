using System;
using System.Collections.Generic;
using System.Linq;
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
        private static string CachePrefix => $"{StoragrConstants.CachePrefix}:USER";
        
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationAdapter _authentication;
        private readonly IBackendAdapter _backendAdapter;
        private readonly IDistributedCache _cache;
        private readonly ITokenService _tokenService;
        private readonly DistributedCacheEntryOptions _cacheEntryOptions;

        public UserService(IBackendAdapter backendAdapter, ITokenService tokenService, IDistributedCache cache, IAuthenticationAdapter authentication, IHttpContextAccessor httpContextAccessor)
        {
            _backendAdapter = backendAdapter;
            _tokenService = tokenService;
            _cache = cache;
            _authentication = authentication;
            _httpContextAccessor = httpContextAccessor;
            _cacheEntryOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // TODO time from a global config
        }

        public async Task<UserEntity> Authenticate(string username, string password)
        {
            var result = await _authentication.Authenticate(username, password);
            if (result is null)
                return null;

            var user = await _backendAdapter.Get<UserEntity>(x =>
            {
                x.Where(f =>
                {
                    f
                        .Equal(nameof(UserEntity.AuthAdapter), _authentication.Name)
                        .And()
                        .Equal(nameof(UserEntity.AuthId), result.Id);
                });
            });
            if (user == null)
            {
                await _backendAdapter.Insert(user = new UserEntity()
                {
                    Id = StoragrHelper.UUID(),
                    IsEnabled = true,
                    IsAdmin = !(await _backendAdapter.GetAll<UserEntity>()).Any(), // first created user is admin
                    AuthAdapter = _authentication.Name,
                    AuthId = result.Id,
                    Username = result.Username,
                });
            }
            else if(user.Username != result.Username)
            {
                user.Username = result.Username;
                await _backendAdapter.Update(user);
            }
            
            var token = new UserToken()
            {
                UserId = user.Id,
                Role = user.IsAdmin ? StoragrConstants.ManagementRole : default
            };
            user.Token = _tokenService.Generate(token) ?? throw new Exception("Unable to generate token!");

            return user;
        }

        public async Task<bool> ExistsByUsername(string username)
        {
            return await _backendAdapter.Get<UserEntity>(x =>
            {
                x.Where(f =>
                {
                    f.Equal(nameof(UserEntity.Username), username);
                });
            }) != null;
        }

        public async Task<UserEntity> Create(string username, string password, bool isAdmin)
        {
            if (!(_authentication is BackendAuthenticator authenticator))
                throw new NotImplementedError();
            if(await ExistsByUsername(username))
                throw new UserAlreadyExistsError(null); // TODO

            var entity = default(UserEntity);
            var result = await authenticator.Create(username, password);

            await _backendAdapter.Insert(entity = new UserEntity()
            {
                Id = StoragrHelper.UUID(),
                IsEnabled = true,
                IsAdmin = isAdmin,
                AuthAdapter = _authentication.Name,
                AuthId = result.Id,
                Username = result.Username,
            });

            return entity;
        }
        
        public async Task Create(UserEntity entity)
        {
            if(await _backendAdapter.Exists<UserEntity>(entity.Id))
                throw new UserAlreadyExistsError(null); // TODO

            await _backendAdapter.Insert(entity);
        }

        public async Task Modify(UserEntity entity, string newPassword)
        {
            var user = await Get(entity.Id);
            if(user is null)
                throw new UserNotFoundError();

            if (_authentication is BackendAuthenticator authenticator)
            {
                await authenticator.Modify(entity.AuthId, entity.Username, newPassword);
            }

            // prevent change of these columns
            entity.Username = user.Username;
            entity.AuthId = user.AuthId;
            entity.AuthAdapter = user.AuthAdapter;
            
            await _backendAdapter.Update(entity);
            await _cache.RemoveAsync($"{CachePrefix}:{entity.Id}");
        }

        public Task<bool> Exists(string id) => _backendAdapter.Exists<UserEntity>(id);
        public Task<int> Count() => _backendAdapter.Count<UserEntity>();
        
        public Task<UserEntity> Get(string id) => _backendAdapter.Get<UserEntity>(id);
        public Task<IEnumerable<UserEntity>> GetMany(IEnumerable<string> entityIds) =>
            _backendAdapter.GetAll<UserEntity>(query =>
            {
                query.Where(filter => filter
                    .In(nameof(UserEntity.Id), entityIds)
                );
            });
        public Task<IEnumerable<UserEntity>> GetAll() =>
            _backendAdapter.GetAll<UserEntity>();

        public Task<IEnumerable<UserEntity>> GetAll(UserSearchArgs searchArgs) =>
            _backendAdapter.GetAll<UserEntity>(q =>
            {
                q.Where(f => f.Like(nameof(UserEntity.Username), searchArgs.Username));
            });

        public async Task<UserEntity> GetAuthenticatedUser()
        {
            var unqiueId = _httpContextAccessor.HttpContext.User.FindFirst(StoragrConstants.TokenUnqiueId)?.Value;
            if (unqiueId is null)
                return null;
            
            var user = default(UserEntity);
            var cacheData = await _cache.GetAsync($"{CachePrefix}:{unqiueId}");
            if (cacheData is not null)
                user = StoragrHelper.DeserializeObject<UserEntity>(cacheData);

            user ??= await _backendAdapter.Get<UserEntity>(unqiueId);
            if (user is not null)
                await _cache.SetAsync($"{CachePrefix}:{unqiueId}", StoragrHelper.SerializeObject(user), _cacheEntryOptions);
            
            return user;
        }

        public async Task<string> GetAuthenticatedToken()
        {
            var token = await _httpContextAccessor.HttpContext.GetTokenAsync("Bearer", "access_token");
            if (string.IsNullOrEmpty(token))
            {
                token = await _httpContextAccessor.HttpContext.GetTokenAsync("Basic", "access_token");
            }

            return token;
        }

        public async Task Delete(string userId)
        {
            if (!(_authentication is BackendAuthenticator authenticator))
                throw new NotImplementedError();

            var user = await Get(userId);
            if (user is null)
                throw new UserNotFoundError();
                
            await authenticator.Delete(user.AuthId);
            await _backendAdapter.Delete(user);
            await _cache.RemoveAsync($"{CachePrefix}:{userId}");
        }
    }
}