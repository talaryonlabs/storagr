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
            var authenticationResult = await _authentication.Authenticate(new AuthenticationRequest()
            {
                Username = username,
                Password = password
            });
            if (authenticationResult == null)
                return null;
            
            var user = await CreateOrUpdate(_authentication.Name, authenticationResult.Id, authenticationResult.Username, authenticationResult.Mail, authenticationResult.Role);
            if (!(user != null && (user.Token = _tokenService.Generate(new UserToken() {UserId = user.UserId, Role = user.Role})) != null))
                return null;

            var data = StoragrHelper.SerializeObject(user);
            await _cache.SetAsync($"TOKEN:{user.Token}", data, _cacheEntryOptions);

            return user;
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
                    IsEnabled = true,
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
            if (_authentication is BackendAuthenticator authenticator)
            {
                // TODO await authenticator.Modify(user.AuthId, modifyRequest.Username, modifyRequest.Password, modifyRequest.Mail, modifyRequest.Role);
            }
            
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
            var unqiueId = _httpContextAccessor.HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
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

        public async Task Delete(string userId)
        {
            if (_authentication is BackendAuthenticator authenticator)
                await authenticator.Delete(userId);
            
            await _backend.Delete(new UserEntity() {UserId = userId});
        }
    }
}