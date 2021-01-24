﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Storagr.Server
{
    public static class StoragrExtensions
    {
        public static IServiceCollection AddStore<TStore>(this IServiceCollection services)
            where TStore : class, IStoreAdapter
        {
            return services
                .AddSingleton<TStore>()
                .AddSingleton<IStoreAdapter>(x => x.GetRequiredService<TStore>());
        }
        public static IServiceCollection AddStore<TStore, TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where TStore : class, IStoreAdapter
            where TOptions : class, IOptions<TOptions>
        {
            return services
                .AddOptions()
                .Configure(configureOptions)
                .AddSingleton<TStore>()
                .AddSingleton<IStoreAdapter>(x => x.GetRequiredService<TStore>());
        }
        
        public static IServiceCollection AddBackend<TBackend>(this IServiceCollection services)
            where TBackend : class, IDatabaseAdapter
        {
            return services
                .AddSingleton<TBackend>()
                .AddSingleton<IDatabaseAdapter>(x => x.GetRequiredService<TBackend>());
        }
        public static IServiceCollection AddBackend<TBackend, TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where TBackend : class, IDatabaseAdapter
            where TOptions : class, IOptions<TOptions>
        {
            return services
                .AddOptions()
                .Configure(configureOptions)
                .AddSingleton<TBackend>()
                .AddSingleton<IDatabaseAdapter>(x => x.GetRequiredService<TBackend>());
        }
        
        public static IServiceCollection AddAuthentication<TAuthentication>(this IServiceCollection services)
            where TAuthentication : class, IAuthenticationAdapter
        {
            return services
                .AddSingleton<TAuthentication>()
                .AddSingleton<IAuthenticationAdapter>(x => x.GetRequiredService<TAuthentication>());
        }
        public static IServiceCollection AddAuthentication<TAuthentication, TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where TAuthentication : class, IAuthenticationAdapter
            where TOptions : class, IOptions<TOptions>
        {
            return services
                .AddOptions()
                .Configure(configureOptions)
                .AddSingleton<TAuthentication>()
                .AddSingleton<IAuthenticationAdapter>(x => x.GetRequiredService<TAuthentication>());
        }
    }

    public static class StoragrCacheExtensions
    {
        public static async Task<bool> ExistsAsync(this IDistributedCache cache, string key, CancellationToken cancellationToken = default) =>
            (await cache.GetAsync(key, cancellationToken)) is not null;

        public static async Task<T> GetObjectAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
        {
            var value = await cache.GetAsync(key, cancellationToken);
            return value is null || cancellationToken.IsCancellationRequested ? default : StoragrHelper.DeserializeObject<T>(value);
        }
        
        public static Task SetObjectAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions cacheEntryOptions = default, CancellationToken cancellationToken = default) =>
            cache.SetAsync(
                key,
                StoragrHelper.SerializeObject(value),
                cacheEntryOptions,
                cancellationToken
            );
    }
    
    public static class StoragrTaskExtensions
    {
        public static T RunSynchronouslyWithResult<T>(this Task<T> task)
        {
            task.RunSynchronously();
            return task.Result;
        }
    }
}