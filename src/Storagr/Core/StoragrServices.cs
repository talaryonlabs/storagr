using System;
using System.Linq;
using System.Text;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Storagr.Data;
using Storagr.Data.Migrations;
using Storagr.IO;
using Storagr.Security;
using Storagr.Security.Authenticators;
using Storagr.Services;
using Storagr.Shared;
using Storagr.Shared.Security;

namespace Storagr
{
    public static class StoragrServices
    {
        public static IServiceCollection AddStoragrCore(this IServiceCollection services, StoragrSettings storagrSettings)
        {
            var featureProvider = new StoragrFeatureProvider(storagrSettings);
            var mediaType = new StoragerMediaTypeHeader();
            var tokenValidationParameters = new StoragrTokenValidationParameters(
                storagrSettings.TokenSettings.Issuer,
                storagrSettings.TokenSettings.Audience, 
                storagrSettings.TokenSettings.Secret);

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddControllers()
                .ConfigureApplicationPartManager(apm =>
                {
                    foreach (var provider in apm.FeatureProviders.OfType<ControllerFeatureProvider>().ToList())
                    {
                        apm.FeatureProviders.Remove(provider);
                    }
                    apm.FeatureProviders.Add(featureProvider);
                });
            services.AddMvcCore()
                .AddMvcOptions(options =>
                {
                    options.Filters.Add(new ProducesAttribute(mediaType.MediaType.Buffer));

                    foreach (var input in options.InputFormatters.OfType<NewtonsoftJsonInputFormatter>())
                    {
                        input.SupportedMediaTypes.Add(mediaType);
                    }

                    foreach (var output in options.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>())
                    {
                        output.SupportedMediaTypes.Add(mediaType);
                    }

                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";
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
                    options.TokenValidationParameters = tokenValidationParameters;
                });
            
            services.AddAuthorization(config =>
            {
                config.AddPolicy("Management", x =>
                {
                    x.RequireAuthenticatedUser();
                    x.RequireRole("Admin");
                });
            });
            
            services.AddTokenService(options =>
            {
                options.ValidationParameters = tokenValidationParameters;
                options.Secret = storagrSettings.TokenSettings.Secret;
                options.Expiration = storagrSettings.TokenSettings.Expiration;
            });

            services.AddAuthentication<BackendAuthenticator>();

            
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IObjectService, ObjectService>();
            services.AddSingleton<ILockService, LockService>();

            return services;
        }

        public static IServiceCollection AddStoragrCache(this IServiceCollection services,  StoragrCacheSettings cacheSettings)
        {
            switch (cacheSettings.Type)
            {
                case StoragrCacheType.Memory:
                    services.AddDistributedMemoryCache(options =>
                    {
                        options.SizeLimit = cacheSettings.SizeLimit;
                    });
                    break;
                case StoragrCacheType.Redis:
                    services.AddDistributedRedisCache(options =>
                    {
                        options.InstanceName = "redis";
                        options.Configuration = cacheSettings.Host;
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return services;
        }

        public static IServiceCollection AddStoragrBackend(this IServiceCollection services, StoragrBackendSettings backendSettings)
        {
            services.AddFluentMigratorCore().ConfigureRunner(config =>
            {
                switch (backendSettings.Type)
                {
                    case StoragrBackendType.Sqlite:
                        config.AddSQLite().WithGlobalConnectionString($"Data Source={backendSettings.DataSource}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                config.ScanIn(typeof(Setup).Assembly).For.Migrations();
            });

            switch (backendSettings.Type)
            {
                case StoragrBackendType.Sqlite:
                    services.AddBackend<SqliteBackend, SqliteBackendOptions>(options =>
                    {
                        options.DataSource = backendSettings.DataSource;
                    });
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return services;
        }

        public static IServiceCollection AddStoragrStore(this IServiceCollection services, StoragrStoreSettings storeSettings)
        {
            switch (storeSettings.Type)
            {
                case StoragrStoreType.Storagr:
                    services.AddStore<StoragrStore, StoragrStoreOptions>(options =>
                    {
                        options.Host = storeSettings.Host;
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return services;
        }
    }
}