using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Shared;

namespace Storagr.Server.Services
{
    public class CacheService :
        ICacheService,
        IStoragrRunner
    {
        private IDistributedCache Cache { get; }
        private DistributedCacheEntryOptions EntryOptions { get; }

        private IEnumerable<string> _deleteKeys;

        public CacheService(IDistributedCache cache)
        {
            Cache = cache;
            EntryOptions = new DistributedCacheEntryOptions();
        }

        ICacheServiceEntry<T> ICacheService.Key<T>(string key)
        {
            return new ServiceEntry<T>(this, key);
        }

        public ICacheServiceEntry<object> Key(string key)
        {
            return new ServiceEntry<object>(this, key);
        }

        IStoragrRunner ICacheService.RemoveMany(IEnumerable<string> keys)
        {
            _deleteKeys = keys;
            return this;
        }
        void IStoragrRunner.Run() =>  (this as IStoragrRunner)
            .RunAsync()
            .RunSynchronously();

        Task IStoragrRunner.RunAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(_deleteKeys
                .Select(key => Cache.RemoveAsync(key, cancellationToken))
            );
        }

        private class ServiceEntry<T> :
            ICacheServiceEntry<T>,
            IStoragrRunner,
            IStoragrRunner<bool>
        {
            private readonly CacheService _service;
            private readonly string _key;

            private T _value;
            private bool _remove;
            private bool _refresh;

            public ServiceEntry(CacheService service, string key)
            {
                _service = service ?? throw new NullReferenceException();
                _key = key ?? throw new NullReferenceException();
            }

            T IStoragrRunner<T>.Run() => (this as IStoragrRunner<T>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<T> IStoragrRunner<T>.RunAsync(CancellationToken cancellationToken)
            {
                var data = await _service
                    .Cache
                    .GetAsync(_key, cancellationToken);

                if (data is null || data.Length == 0)
                    return default;

                return StoragrHelper.DeserializeObject<T>(data);
            }


            IStoragrRunner ICacheServiceEntry<T>.Set(T value)
            {
                _value = value ?? throw new NullReferenceException();
                return this;
            }

            IStoragrRunner ICacheServiceEntry<T>.Refresh(T value)
            {
                _refresh = true;
                return this;
            }

            IStoragrRunner IStoragrDeletable.Delete(bool force)
            {
                _remove = true;
                return this;
            }

            void IStoragrRunner.Run() => (this as IStoragrRunner)
                .RunAsync()
                .RunSynchronously();

            Task IStoragrRunner.RunAsync(CancellationToken cancellationToken) =>
                _remove
                    ? _service
                        .Cache
                        .RemoveAsync(
                            _key,
                            cancellationToken
                        )
                    : _refresh
                        ? _service
                            .Cache
                            .RefreshAsync(_key, cancellationToken)
                        : _service
                            .Cache
                            .SetAsync(
                                _key,
                                StoragrHelper.SerializeObject(_value),
                                _service.EntryOptions,
                                cancellationToken
                            );

            IStoragrRunner<bool> IStoragrExistable.Exists() => this;

            bool IStoragrRunner<bool>.Run() => (this as IStoragrRunner<bool>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<bool> IStoragrRunner<bool>.RunAsync(CancellationToken cancellationToken)
            {
                var value = await _service
                    .Cache
                    .GetAsync(_key, cancellationToken);
                return value is not null && value.Length != 0;
            }
        }
    }
}