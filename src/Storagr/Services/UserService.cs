using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.Security;

namespace Storagr.Services
{
    public interface IAuthenticationResult // TODO move to other location ... or replace/rename?
    {
        string UserId { get; set; }
        string Username { get; set; }
        string Role { get; set; }
        string Token { get; set; }
    }
    
    public interface IUserService
    {
        Task<UserEntity> GetAuthenticatedUser();
        Task<string> GetAuthenticatedUserToken();
        
        Task<UserEntity> GetUserByName(string username);
        Task<IAuthenticationResult> Authenticate(string username, string password);
        Task<IAuthenticationResult> Authenticate(string token);

        Task<UserEntity> CreateOrUpdate(string authAdapater, string authId, string username, string mail, string role);
        Task Modify(UserEntity entity);
        Task Delete(string userId);

        Task<UserEntity> Get(string userId);
        Task<IEnumerable<UserEntity>> GetAll();
    }

    public class UserService : IDisposable, IUserService
    {
        private class AuthenticationResult : IAuthenticationResult
        {
            public string UserId { get; set; }
            public string Username { get; set; }
            public string Token { get; set; }
            public string Role { get; set; }
        }
        
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

        public async Task<IAuthenticationResult> Authenticate(string username, string password)
        {
            var authenticationResult = await _authentication.Authenticate(new AuthenticationRequest()
            {
                Username = username,
                Password = password
            });
            if (authenticationResult == null)
                return null;
            
            var user = await CreateOrUpdate(_authentication.Name, authenticationResult.Id, authenticationResult.Username, authenticationResult.Mail, authenticationResult.Role);

            var token = _tokenService.Generate(new TokenData() {UniqueId = user.UserId, Role = user.Role});
            if (token == null)
                return null;

            var data = StoragrHelper.SerializeObject(user);
            await _cache.SetAsync($"TOKEN:{token}", data, _cacheEntryOptions);

            return new AuthenticationResult()
            {
                UserId = user.UserId,
                Username =  user.Username,
                Role = user.Role,
                Token = token
            };
        }

        public async Task<IAuthenticationResult> Authenticate(string token)
        {
            var cachedData = await _cache.GetAsync($"TOKEN:{token}");
            if (cachedData == null)
                return null;

            var user = StoragrHelper.DeserializeObject<UserEntity>(cachedData);
            if (user == null || !_tokenService.Verify(token, new TokenData() {UniqueId = user.UserId}))
                return null;

            // TODO (optional) refresh token?
            return new AuthenticationResult()
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role,
                Token = token
            };
        }

        public async Task<UserEntity> CreateOrUpdate(string authAdapater, string authId, string username, string mail, string role)
        {
            var user = await _backend.Get<UserEntity>(x =>
            {
                x.Where(f => f.Equal("AuthAdapter", authAdapater).And().Equal("AuthId", authId));
            });

            if (user == null)
            {
                await _backend.Insert(user = new UserEntity()
                {
                    UserId = StoragrHelper.UUID(),
                    AuthAdapter = authAdapater,
                    AuthId = authId,
                    Username = username,
                    Mail = mail ?? "",
                    Role = role ?? ""
                });
            }

            user.Username = username ?? user.Username;
            user.Mail = mail ?? user.Mail;
            user.Role = role ?? user.Role;
            
            await _backend.Update(user);

            return user;
        }

        public async Task Modify(UserEntity entity)
        {
            await _backend.Update(entity);
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
            var unqiueId = _httpContextAccessor.HttpContext.User.FindFirst("UniqueId")?.Value;
            if (unqiueId == null)
                return null;
            
            var user = default(UserEntity);
            var key = $"USER:UID:{unqiueId}";
            var cacheData = await _cache.GetAsync(key);
            if (cacheData != null)
                user = StoragrHelper.DeserializeObject<UserEntity>(cacheData);

            user ??= await _backend.Get<UserEntity>(unqiueId);
            if (user != null)
                await _cache.SetAsync(key, StoragrHelper.SerializeObject(user), _cacheEntryOptions);
            
            return user;
        }

        public async Task<string> GetAuthenticatedUserToken()
        {
            var token = await _httpContextAccessor.HttpContext.GetTokenAsync("Bearer", "access_token");
            if (string.IsNullOrEmpty(token))
            {
                token = await _httpContextAccessor.HttpContext.GetTokenAsync("Basic", "access_token");
            }

            return token;
        }

        public async Task<UserEntity> GetUserByName(string username) => await _backend.Get<UserEntity>(x => x.Where(f => f.Equal("Username", username)));
        
        public async Task Delete(string userId) => await _backend.Delete(new UserEntity() {UserId = userId});
        
        public void Dispose()
        {
        }
    }
}