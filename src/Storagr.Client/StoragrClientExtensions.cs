using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Storagr;

namespace Storagr.Client
{
    public class StoragrClientOptions : StoragrOptions<StoragrClientOptions>
    {
        public string Host { get; set; }
        public string Token { get; set; }
    }
    
    public static class StoragrClientExtensions
    {
        public static IServiceCollection AddStoragrClient(this IServiceCollection services, Action<StoragrClientOptions> configureOptions)
        {
            services
                .AddOptions()
                .Configure(configureOptions)
                .AddHttpClient()
                .AddScoped(provider =>
                {
                    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                    var options = provider.GetRequiredService<IOptions<StoragrClientOptions>>().Value;

                    return new StoragrClient(httpClientFactory.CreateClient())
                        .UseHostname(options.Host)
                        .UseToken(options.Token);
                });
            
            return services;
        }
    }
}