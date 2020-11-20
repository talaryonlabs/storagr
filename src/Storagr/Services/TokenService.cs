using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Storagr.Services
{
    public interface ITokenService
    {
        string Generate(TokenData data);
        string Refresh(string token);
        bool Verify(string token, TokenData data);

        TokenData Get(string token);
    }
    
    public class TokenServiceOptions : StoragrOptions<TokenServiceOptions>
    {
        public TokenValidationParameters ValidationParameters { get; set; }
        public string Secret { get; set; }
        public int Expiration { get; set; }
    }

    public class TokenData
    {
        public string UniqueId { get; set; }
        public string Role { get; set; }
    }

    public static class TokenServiceExtension
    {
        public static IServiceCollection AddTokenService(this IServiceCollection services, Action<TokenServiceOptions> configureOptions)
        {
            return services
                .AddOptions()
                .Configure(configureOptions)
                .AddSingleton<TokenService>()
                .AddSingleton<ITokenService>(x => x.GetRequiredService<TokenService>());
        }
    }
    
    public class TokenService : ITokenService, IDisposable
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

        public string Generate(TokenData data)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, data.UniqueId),
                new Claim("UniqueId", data.UniqueId),
                new Claim(ClaimTypes.Role, data.Role), 
                new Claim(JwtRegisteredClaimNames.Jti, StoragrHelper.UUID())
            };
            var token = new JwtSecurityToken(
                issuer: _options.ValidationParameters.ValidIssuer,
                audience: _options.ValidationParameters.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddSeconds(_options.Expiration),
                signingCredentials: new SigningCredentials(_options.ValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return _securityTokenHandler.WriteToken(token);
        }

        public string Refresh(string token)
        {
            throw new NotImplementedException();
        }

        public bool Verify(string token, TokenData data)
        {
            ClaimsPrincipal principal;
            try
            {
                principal = _securityTokenHandler.ValidateToken(token, _options.ValidationParameters, out var validatedToken);
            }
            catch (Exception)
            {
                return false;
            }
            return data.UniqueId == principal.Claims.First(c => c.Type == "UniqueId").Value;
        }

        public TokenData Get(string token)
        {
            var securityToken = _securityTokenHandler.ReadJwtToken(token);

            return new TokenData()
            {
                UniqueId = securityToken.Claims.First(c => c.Type == "UniqueId").Value,
                Role = securityToken.Claims.First(c=> c.Type == ClaimTypes.Role).Value
            };
        }


        public void Dispose()
        {
        }
    }
}