using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Storagr
{
    public static class StoragrExtensions
    {
        public static IServiceCollection AddConfig<TConfig>(this IServiceCollection services, StoragrConfig config)
            where TConfig : class, IOptions<TConfig>, new()
        {
            var conf = config.Get<TConfig>();
            
            return services
                .AddSingleton<IOptions<TConfig>>(config.Get<TConfig>());
        }
        
        public static IServiceCollection AddSingleton<TService, TImplementation, TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where TService : class
            where TImplementation : class, TService
            where TOptions : class, IOptions<TOptions>, new()
        {
            return services
                .AddOptions()
                .Configure(configureOptions)
                .AddSingleton<TImplementation>()
                .AddSingleton<TService>(x => x.GetRequiredService<TImplementation>());
        }
        
        public static IServiceCollection AddScoped<TService, TImplementation, TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where TService : class
            where TImplementation : class, TService
            where TOptions : class, IOptions<TOptions>, new()
        {
            return services
                .AddOptions()
                .Configure(configureOptions)
                .AddScoped<TImplementation>()
                .AddScoped<TService>(x => x.GetRequiredService<TImplementation>());
        }
        
        public static IServiceCollection AddTransient<TService, TImplementation, TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where TService : class
            where TImplementation : class, TService
            where TOptions : class, IOptions<TOptions>, new()
        {
            return services
                .AddOptions()
                .Configure(configureOptions)
                .AddTransient<TImplementation>()
                .AddTransient<TService>(x => x.GetRequiredService<TImplementation>());
        }
    }
}