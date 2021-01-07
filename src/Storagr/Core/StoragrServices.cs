using System;
using System.Linq;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
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
        public static IServiceCollection AddStoragrCore(this IServiceCollection services, StoragrConfig config)
        {
            var mediaType = new StoragrMediaType();
            var storagrConfig = config.Get<StoragrCoreConfig>();

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Listen(storagrConfig.Listen);
            });
            
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    mediaType.MediaType.Value
                });
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

            services.AddSingleton<IRepositoryService, RepositoryService>();
            services.AddSingleton<IObjectService, ObjectService>();
            services.AddSingleton<ILockService, LockService>();
            services.AddSingleton<IUserService, UserService>();
            
            return services;
        }

        public static IServiceCollection AddStoragrSecurity(this IServiceCollection services, StoragrConfig config)
        {
            var tokenConfig = config.Get<TokenOptions>();

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
                    options.TokenValidationParameters = tokenConfig;
                });
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy(StoragrConstants.ManagementPolicy, x =>
                {
                    x.RequireAuthenticatedUser();
                    x.RequireRole(StoragrConstants.ManagementRole);
                });
            });

            services.AddConfig<TokenOptions>(config);
            services.AddSingleton<ITokenService, TokenService>();

            services.AddAuthentication<BackendAuthenticator>();

            return services;
        }

        public static IServiceCollection AddStoragrCache(this IServiceCollection services,  StoragrConfig config)
        {
            var storagrConfig = config.Get<StoragrCoreConfig>();
            
            switch (storagrConfig.Cache)
            {
                case StoragrCacheType.Memory:
                    services.AddDistributedMemoryCache();
                    break;
                
                case StoragrCacheType.Redis:
                    var redisConfig = config.Get<RedisCacheConfig>();
                    
                    services.AddDistributedRedisCache(options =>
                    {
                        options.InstanceName = "redis";
                        options.Configuration = redisConfig.Host;
                    });
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return services;
        }

        public static IServiceCollection AddStoragrBackend(this IServiceCollection services, StoragrConfig config)
        {
            var storagrConfig = config.Get<StoragrCoreConfig>();
            
            services.AddFluentMigratorCore().ConfigureRunner(options =>
            {
                switch (storagrConfig.Backend)
                {
                    case StoragrBackendType.Sqlite:
                        var sqliteConfig = config.Get<SqliteOptions>();
                        options.AddSQLite().WithGlobalConnectionString($"Data Source={sqliteConfig.DataSource}");
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                options.ScanIn(typeof(Setup).Assembly).For.Migrations();
            });

            switch (storagrConfig.Backend)
            {
                case StoragrBackendType.Sqlite:
                    services.AddConfig<SqliteOptions>(config);
                    services.AddBackend<SqliteAdapter>();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return services;
        }

        public static IServiceCollection AddStoragrStore(this IServiceCollection services, StoragrConfig config)
        {
            var storagrConfig = config.Get<StoragrCoreConfig>();
            
            switch (storagrConfig.Store)
            {
                case StoragrStoreType.Storagr:
                    services.AddConfig<StoragrStoreOptions>(config);
                    services.AddStore<StoragrStore>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return services;
        }
    }
}