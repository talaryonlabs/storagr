using System;
using System.Linq;
using System.Text;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Storagr.Data;
using Storagr.IO;
using Storagr.Security;
using Storagr.Security.Authenticators;
using Storagr.Services;

namespace Storagr
{
    public static class StoragrServices
    {
        public static IServiceCollection AddStoragrCore(this IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true; // TODO remove this at production!
            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisismySecretKey"));
            
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddMvcCore().AddMvcOptions(options =>
            {
                var mediaType = new LfsMediaTypeHeader();

                options.Filters.Add(new ProducesAttribute(mediaType.MediaType.Buffer));

                foreach (var input in options.InputFormatters.OfType<NewtonsoftJsonInputFormatter>())
                {
                    input.SupportedMediaTypes.Add(mediaType);
                }

                foreach (var output in options.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>())
                {
                    output.SupportedMediaTypes.Add(mediaType);
                }
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddAuthentication("_")
                .AddPolicyScheme("_", "AuthRouter", options =>
                {
                    options.ForwardDefaultSelector = ctx =>
                    {
                        if (ctx.Request.Headers.ContainsKey(HeaderNames.Authorization) && ((string)ctx.Request.Headers[HeaderNames.Authorization]).StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            return JwtBearerDefaults.AuthenticationScheme;
                        }
                        return BasicAuthenticationDefaults.AuthenticationScheme;
                    };
                })
                .AddBasic(options =>
                {

                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = securityKey,

                    };
                });
            
            services.AddAuthorization(config =>
            {
                config.AddPolicy("Management", x =>
                {
                    x.RequireAuthenticatedUser();
                    x.RequireRole("Admin");
                });
            });

            services.AddBackendAuthenticator();

            
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IObjectService, ObjectService>();
            services.AddSingleton<ILockService, LockService>();

            return services;
        }

        public static IServiceCollection AddStoragrCache(this IServiceCollection services)
        {
            // TODO setup cache with ENV variable
            
            services.AddDistributedMemoryCache(options =>
            {
                options.SizeLimit = 4096;
            });
            
            // services.AddDistributedRedisCache(options =>
            // {
            //     options.InstanceName = "redis";
            //     options.Configuration = "localhost:6379";
            // });
            
            return services;
        }

        public static IServiceCollection AddStoragrBackend(this IServiceCollection services)
        {
            // TODO setup backend with ENV variable
            
            services.AddFluentMigratorCore().ConfigureRunner(config =>
            {
                config.AddSQLite().WithGlobalConnectionString(@"Data Source=storagr1.db");
                config.ScanIn(typeof(InitialSetup).Assembly).For.Migrations();
            });
            
            services.AddSqliteBackend(options =>
            {
                options.DataSource = "storagr1.db";
            });
            return services;
        }

        public static IServiceCollection AddStoragrStore(this IServiceCollection services)
        {
            // TODO set RootPath with ENV variable
            
            services.AddLocalStore(options =>
            {
                options.RootPath = "D:/store";
            });
            return services;
        }
    }
}