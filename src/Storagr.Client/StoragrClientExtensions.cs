using System;
using Microsoft.Extensions.DependencyInjection;
using Storagr.Shared;

namespace Storagr.Client
{
    public class StoragrClientOptions : StoragrOptions<StoragrClientOptions>
    {
        public string Host { get; set; }
        
        public string Token { get; set; }

        public short DefaultPort { get; set; } = 80;
        public string DefaultProtocol { get; set; } = "https";
    }
    
    public static class StoragrClientExtensions
    {
        public static IServiceCollection AddStoragrClient(this IServiceCollection services, Action<StoragrClientOptions> configureOptions)
        {
            services
                .AddOptions()
                .Configure(configureOptions)
                .AddHttpClient()
                .AddScoped<IStoragrClient, StoragrClient>();
            
            return services;
        }
    }
}