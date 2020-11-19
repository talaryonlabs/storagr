using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

    public class TokenData
    {
        public string UniqueId { get; set; }
        public string Role { get; set; }
    }
    
    public class TokenService : ITokenService, IDisposable
    {
        private readonly JwtBearerOptions _bearerOptions;
        private readonly SigningCredentials _signingCredentials;
        private readonly JwtSecurityTokenHandler _securityTokenHandler;

        private readonly int _expiresIn;

        public TokenService(IOptionsMonitor<JwtBearerOptions> optionsAccessor)
        {
            if((_bearerOptions = optionsAccessor.Get(JwtBearerDefaults.AuthenticationScheme)) == null)
                throw new ArgumentNullException(nameof(optionsAccessor));
            
            _expiresIn = 3600; // TODO time from a global config
            _signingCredentials = new SigningCredentials(_bearerOptions.TokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256);
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
                issuer: _bearerOptions.TokenValidationParameters.ValidIssuer,
                audience: _bearerOptions.TokenValidationParameters.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddSeconds(_expiresIn),
                signingCredentials: _signingCredentials
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
                principal = _securityTokenHandler.ValidateToken(token, _bearerOptions.TokenValidationParameters, out var validatedToken);
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