using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Storagr.Shared;
using Storagr.Shared.Security;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Storagr.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSecurityTokenHandler _securityTokenHandler;
        private readonly TokenServiceOptions _options;
        private readonly IDistributedCache _cache;

        public TokenService(IOptions<TokenServiceOptions> optionsAccessor, IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
            _securityTokenHandler = new JwtSecurityTokenHandler();
        }

        public string Generate<T>(T token) where T : class => 
            Generate(token, _options.Expiration);
        public string Generate<T>(T token, TimeSpan expiresIn) where T : class
        {
            var claims = typeof(T)
                .GetProperties()
                .Where(v => v.CanRead && v.GetCustomAttribute(typeof(StoragrTokenMemberAttribute)) != null)
                .SelectMany(v =>
                {
                    var attribute = (StoragrTokenMemberAttribute) v.GetCustomAttribute(typeof(StoragrTokenMemberAttribute));
                    var claim = new Claim(attribute?.Name ?? v.Name, (string) v.GetValue(token));

                    return !string.IsNullOrEmpty(attribute?.ClaimType)
                        ? new[] {claim, new Claim(attribute?.ClaimType, (string) v.GetValue(token))}
                        : new[] {claim};
                })
                .Concat(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, StoragrHelper.UUID())
                });

            var securityToken = new JwtSecurityToken(
                issuer: _options.ValidationParameters.ValidIssuer,
                audience: _options.ValidationParameters.ValidAudience,
                claims: claims,
                expires: DateTime.Now.Add(expiresIn),
                signingCredentials: new SigningCredentials(_options.ValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return _securityTokenHandler.WriteToken(securityToken);
        }

        public string Refresh(string token)
        {
            throw new NotImplementedException();
        }

        public bool Verify<T>(string encodedToken, T token) where T : class
        {
            // ClaimsPrincipal principal;
            // try
            // {
            //     principal = _securityTokenHandler.ValidateToken(token, _options.ValidationParameters, out var validatedToken);
            // }
            // catch (Exception)
            // {
            //     return false;
            // }
            // return data.UniqueId == principal.Claims.First(c => c.Type == "UniqueId").Value;
            
            throw new NotImplementedException();
        }

        public T Get<T>(string encodedToken) where T : class, new()
        {
            var securityToken = _securityTokenHandler.ReadJwtToken(encodedToken);
            var tokenData = Activator.CreateInstance<T>();
            var tokenProperties = typeof(T)
                .GetProperties()
                .Where(v => v.CanWrite && v.GetCustomAttribute(typeof(StoragrTokenMemberAttribute)) != null);

            foreach (var tokenMember in tokenProperties)
            {
                var attribute = (StoragrTokenMemberAttribute) tokenMember.GetCustomAttribute(typeof(StoragrTokenMemberAttribute));
                var claim = securityToken.Claims.First(c => c.Type == attribute?.Name);
                
                tokenMember.SetValue(tokenData, claim?.Value);
            }
            return tokenData;
        }
    }
}