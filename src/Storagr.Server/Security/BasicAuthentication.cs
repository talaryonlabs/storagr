using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Storagr.Server.Security;
using Storagr.Server.Services;

namespace Storagr.Server.Security
{
    public static class BasicAuthenticationDefaults
    {
        /// <summary>
        /// Default value for AuthenticationScheme property in the BasicAuthenticationOptions
        /// </summary>
        public const string AuthenticationScheme = "Basic";
    }
    
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
        
    }

    public static class BasicAuthenticationExtension
    {
        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder) =>
            AddBasic(builder, BasicAuthenticationDefaults.AuthenticationScheme, null, _ => { });
    
        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string authenticationScheme) =>
            AddBasic(builder, authenticationScheme, null, _ => { });
    
        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, Action<AuthenticationSchemeOptions> configureOptions) =>
            AddBasic(builder, BasicAuthenticationDefaults.AuthenticationScheme, null, configureOptions);
    
        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string authenticationScheme, Action<AuthenticationSchemeOptions> configureOptions) =>
            AddBasic(builder, authenticationScheme, null, configureOptions);
    
        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BasicAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }

    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private readonly IStoragrService _storagrService;

        public BasicAuthenticationHandler(IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IStoragrService storagrService) :
            base(options, logger, encoder, clock)
        {
            _storagrService = storagrService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Context.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() is not null)
                return AuthenticateResult.NoResult();

            if(!(Request.Headers.ContainsKey(HeaderNames.Authorization) && AuthenticationHeaderValue.TryParse(Request.Headers[HeaderNames.Authorization], out var header) && header.Scheme == Scheme.Name))
                return AuthenticateResult.NoResult();

            string[] credentialParams;
            try
            {
                var credentialData = Convert.FromBase64String(header.Parameter);
                var credentialString = Encoding.UTF8.GetString(credentialData);

                credentialParams = credentialString.Split(new[] {':'}, 2);
            }
            catch (Exception e)
            {
                return AuthenticateResult.Fail(e);
            }

            var user = await _storagrService
                .Authenticate(credentialParams[0], credentialParams[1])
                .RunAsync();
            if (user is null)
            {
                return AuthenticateResult.Fail("Username or Password invalid.");
            }

            var properties = new AuthenticationProperties();
            properties.StoreTokens(new[]
            {
                new AuthenticationToken {Name = "access_token", Value = user.Token}
            });

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(StoragrConstants.TokenUnqiueId, user.Id),
                new Claim(ClaimTypes.Role, user.IsAdmin ? StoragrConstants.ManagementRole : "")
            }, Scheme.Name);
            
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, properties, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.Headers.Append(HeaderNames.WWWAuthenticate, "Basic realm=\"Storagr.Server\", charset=\"UTF-8\"");
            Response.Headers.Append("LFS-Authenticate", "Basic realm=\"Storagr.Server\", charset=\"UTF-8\"");

            return Task.CompletedTask;
        }
    }
}