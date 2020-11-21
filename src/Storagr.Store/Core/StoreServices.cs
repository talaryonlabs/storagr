using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Storagr.Shared;
using Storagr.Shared.Security;
using Storagr.Store.Services;

namespace Storagr.Store
{
    public static class StoreServices
    {
        public static IServiceCollection AddStoreCore(this IServiceCollection services, StoragrSettings storagrSettings)
        {
            var mediaType = new StoragerMediaTypeHeader();

            var test = mediaType.MediaType.Value;
            
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
            
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new StoragrTokenValidationParameters(
                        storagrSettings.TokenSettings.Issuer, 
                        storagrSettings.TokenSettings.Audience, 
                        storagrSettings.TokenSettings.Secret);
                });
            services.AddAuthorization();
            
            return services;
        }

        public static IServiceCollection AddStoreCache(this IServiceCollection services, StoragrCacheSettings cacheSettings)
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

        public static IServiceCollection AddStoreServices(this IServiceCollection services, StoragrStoreSettings storeSettings)
        {
            services.AddSingleton<IStoreService, StoreService, StoreServiceOptions>(options =>
            {
                options.RootPath = storeSettings.RootPath;
                options.BufferSize = 4096;
                options.Expiration = TimeSpan.FromHours(1);
                options.ScanInterval = TimeSpan.FromMinutes(30);
            });

            return services;
        }
    }
}