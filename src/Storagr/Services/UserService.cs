using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.JsonWebTokens;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.Security;
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
        private readonly IBackendAdapter _backend;
        private readonly IDistributedCache _cache;
        private readonly ITokenService _tokenService;
        private readonly DistributedCacheEntryOptions _cacheEntryOptions;

        public UserService(IBackendAdapter backend, ITokenService tokenService, IDistributedCache cache, IAuthenticationAdapter authentication, IHttpContextAccessor httpContextAccessor)
        {
            _backend = backend;
            _tokenService = tokenService;
            _cache = cache;
            _authentication = authentication;
            _httpContextAccessor = httpContextAccessor;
            _cacheEntryOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // TODO time from a global config
        }

        public async Task<UserEntity> Authenticate(string username, string password)
        {
            var result = await _authentication.Authenticate(username, password);
            if (result == null)
                return null;

            var user = await _backend.Get<UserEntity>(x =>
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
                await _backend.Insert(user = new UserEntity()
                {
                    Id = StoragrHelper.UUID(),
                    IsEnabled = true,
                    IsAdmin = !(await _backend.GetAll<UserEntity>()).Any(), // first created user is admin
                    AuthAdapter = _authentication.Name,
                    AuthId = result.Id,
                    Username = result.Username,
                });
            }
            else if(user.Username != result.Username)
            {
                user.Username = result.Username;
                await _backend.Update(user);
            }
            
            var token = new UserToken()
            {
                UserId = user.Id,
                Role = user.IsAdmin ? StoragrConstants.ManagementRole : default
            };
            user.Token = _tokenService.Generate(token) ?? throw new Exception("Unable to generate token!");

            return user;
        }

        public async Task<bool> Exists(string username)
        {
            return await _backend.Get<UserEntity>(x =>
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
                throw new NotSupportedException();
            if(await Exists(username))
                throw new UserAlreadyExistsException();

            var entity = default(UserEntity);
            var result = await authenticator.Create(username, password);

            await _backend.Insert(entity = new UserEntity()
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

        public async Task Modify(UserEntity entity, string newPassword)
        {
            var user = await Get(entity.Id);
            if(user == null)
                throw new UserNotFoundException();

            if (_authentication is BackendAuthenticator authenticator)
            {
                await authenticator.Modify(entity.AuthId, entity.Username, newPassword);
            }

            // prevent change of these columns
            entity.Username = user.Username;
            entity.AuthId = user.AuthId;
            entity.AuthAdapter = user.AuthAdapter;
            
            await _backend.Update(entity);
            await _cache.RemoveAsync($"{CachePrefix}:{entity.Id}");
        }


        public async Task<UserEntity> Get(string userId)
        {
            return await _backend.Get<UserEntity>(userId);
        }

        public async Task<IEnumerable<UserEntity>> GetAll()
        {
            return await _backend.GetAll<UserEntity>();
        }

        public async Task<UserEntity> GetAuthenticatedUser()
        {
            var unqiueId = _httpContextAccessor.HttpContext.User.FindFirst(StoragrConstants.TokenUnqiueId)?.Value;
            if (unqiueId == null)
                return null;
            
            var user = default(UserEntity);
            var cacheData = await _cache.GetAsync($"{CachePrefix}:{unqiueId}");
            if (cacheData != null)
                user = StoragrHelper.DeserializeObject<UserEntity>(cacheData);

            user ??= await _backend.Get<UserEntity>(unqiueId);
            if (user != null)
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
        
        public async Task<bool> HasAccess(RepositoryEntity repository, RepositoryAccessType accessType)
        {
            var user = await GetAuthenticatedUser();
            var access = user.IsAdmin;
            
            // TODO check for read/write access
            // access |= ...
            access |= repository.OwnerId == user.Id;
            
            return access;
        }

        public async Task Delete(string userId)
        {
            if (!(_authentication is BackendAuthenticator authenticator))
                throw new NotSupportedException();

            var user = await Get(userId);
            if (user == null)
                throw new UserNotFoundException();
                
            await authenticator.Delete(user.AuthId);
            await _backend.Delete(user);
            await _cache.RemoveAsync($"{CachePrefix}:{userId}");
        }
    }
}