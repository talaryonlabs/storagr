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
        private readonly TokenOptions _options;
        private readonly IDistributedCache _cache;

        public TokenService(IOptions<TokenOptions> optionsAccessor, IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(TokenOptions));
            _securityTokenHandler = new JwtSecurityTokenHandler();
        }

        public string Generate<T>(T token) where T : class => 
            Generate(token, _options.Expiration);
        public string Generate<T>(T token, TimeSpan expiresIn) where T : class
        {
            var claims = typeof(T)
                .GetProperties()
                .Where(v => v.CanRead && v.GetCustomAttributes<TokenClaimAttribute>().Any())
                .SelectMany(v =>
                {
                    return v
                        .GetCustomAttributes<TokenClaimAttribute>(true)
                        .Select(a => new Claim(a.Name, (string) v.GetValue(token)));
                })
                .Concat(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, StoragrHelper.UUID())
                }).ToList();

            var parameters = (StoragrTokenParameters) _options;
            
            var securityToken = new JwtSecurityToken(
                issuer: parameters.ValidIssuer,
                audience: parameters.ValidAudience,
                claims: claims,
                expires: DateTime.Now.Add(expiresIn),
                signingCredentials: new SigningCredentials(parameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return _securityTokenHandler.WriteToken(securityToken);
        }

        public T Get<T>(string encodedToken) where T : class, new()
        {
            var securityToken = _securityTokenHandler.ReadJwtToken(encodedToken);
            var tokenData = Activator.CreateInstance<T>();
            var tokenProperties = typeof(T)
                .GetProperties()
                .Where(v => v.CanWrite && v.GetCustomAttributes<TokenClaimAttribute>().Any());

            foreach (var tokenMember in tokenProperties)
            {
                var claim = tokenMember.GetCustomAttributes<TokenClaimAttribute>().Aggregate(
                    default(Claim),
                    (current, attribute) => current ?? securityToken.Claims.First(c => c.Type == attribute?.Name)
                );
                tokenMember.SetValue(tokenData, claim?.Value);
            }
            return tokenData;
        }
    }
}