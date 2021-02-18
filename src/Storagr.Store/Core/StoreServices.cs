using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Storagr;
using Storagr.Shared;
using Storagr.Store.Services;

namespace Storagr.Store
{
    public static class StoreServices
    {
        public static IServiceCollection AddStoreCore(this IServiceCollection services, StoragrConfig config)
        {
            var mediaType = new StoragrMediaType();
            var storeConfig = config.Get<StoreConfig>();

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Listen(storeConfig.Listen);
            });
            
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
                    options.Filters.Add(new StoragrExceptionFilter(mediaType));
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
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = config.Get<TokenConfig>();
                });
            services.AddAuthorization();
            
            return services;
        }

        public static IServiceCollection AddStoreServices(this IServiceCollection services, StoragrConfig config)
        {
            return services
                .AddConfig<StoreConfig>(config)
                .AddSingleton<IStoreService, StoreService>();
        }
    }
}