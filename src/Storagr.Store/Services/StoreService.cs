using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Services
{
    public class StoreService : BackgroundService, IStoreService
    {
        public int BufferSize => _options.BufferSize;

        private readonly IDistributedCache _cache;
        private readonly StoreConfig _options;
        private readonly DirectoryInfo _rootDirectory;
        private readonly DirectoryInfo _tmpDirectory;
        private readonly DistributedCacheEntryOptions _cacheEntryOptions;

        public StoreService(IOptions<StoreConfig> optionsAccessor, IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(IDistributedCache), "Cache is null.");
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(StoreConfig));
            _rootDirectory = new DirectoryInfo(_options.RootPath);
            _tmpDirectory = new DirectoryInfo(Path.Combine(_options.RootPath, ".tmp"));

            _cacheEntryOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(_options.Expiration);

            if (!_rootDirectory.Exists)
                _rootDirectory.Create();

            if (!_tmpDirectory.Exists)
                _tmpDirectory.Create();
        }
        
        private string GetObjectPath(string repositoryId, string objectId)
        {
            if (string.IsNullOrEmpty(repositoryId)) throw new ArgumentNullException(nameof(repositoryId));
            if (string.IsNullOrEmpty(objectId)) throw new ArgumentNullException(nameof(objectId));

            return Path.Combine(new[]
            {
                GetRepositoryPath(repositoryId),
                objectId.Substring(0, 2),
                objectId.Substring(2, 2),
                objectId
            });
        }

        private string GetRepositoryPath(string repositoryId)
        {
            if (string.IsNullOrEmpty(repositoryId)) throw new ArgumentNullException(nameof(repositoryId));
            
            return Path.Combine(new[]
            {
                _rootDirectory.FullName,
                repositoryId
            });
        }

        private string CreateTemporaryFile()
        {
            return Path.Combine(_tmpDirectory.FullName, Path.GetRandomFileName());
        }

        public bool Exists(string repositoryId) => 
            Directory.Exists(GetRepositoryPath(repositoryId));
        public bool Exists(string repositoryId, string objectId) => 
            File.Exists(GetObjectPath(repositoryId, objectId));

        public StoreRepository Get(string repositoryId)
        {
            var path = GetRepositoryPath(repositoryId);
            if (!Directory.Exists(path))
                throw new RepositoryNotFoundError();

            return new StoreRepository()
            {
                RepositoryId = repositoryId,
                UsedSpace = new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(v => v.Length)
            };
        }
        public StoreObject Get(string repositoryId, string objectId)
        {
            var path = GetObjectPath(repositoryId, objectId);
            var file = new FileInfo(path);
            if(!file.Exists)
                throw new ObjectNotFoundError();

            return new StoreObject()
            {
                ObjectId = objectId,
                RepositoryId = repositoryId,
                Size = file.Length
            };
        }
        
        public IEnumerable<StoreRepository> List() => 
            _rootDirectory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).Where(v => !v.Name.StartsWith(".")).Select(v => Get(v.Name));
        public IEnumerable<StoreObject> List(string repositoryId)
        {
            var path = GetRepositoryPath(repositoryId);
            if (!Directory.Exists(path))
                throw new RepositoryNotFoundError();
            
            return new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Select(v => new StoreObject()
                {
                    ObjectId = v.Name,
                    RepositoryId = repositoryId,
                    Size = v.Length
                });
        }

        public void Delete(string repositoryId)
        {
            var path = GetRepositoryPath(repositoryId);
            if (!Directory.Exists(path))
                throw new RepositoryNotFoundError();
            
            Directory.Delete(path, true);
        }
        public void Delete(string repositoryId, string objectId)
        {
            var path = GetObjectPath(repositoryId, objectId);
            if (!File.Exists(path))
                throw new ObjectNotFoundError();
            
            File.Delete(path);
        }

        public Stream GetDownloadStream(string repositoryId, string objectId)
        {
            var path = GetObjectPath(repositoryId, objectId);
            if (!File.Exists(path))
                throw new ObjectNotFoundError();

            return File.OpenRead(path);
        }

        public Stream GetUploadStream(string repositoryId, string objectId)
        {
            var path = GetObjectPath(repositoryId, objectId);
            var file = new FileInfo(path);
            if (file.Exists)
            {
                throw new ObjectAlreadyExistsError(new StoragrObject()
                {
                    ObjectId = objectId,
                    RepositoryId = repositoryId,
                    Size = file.Length
                });
            }
            var key = StoreCaching.GetTempFileKey(repositoryId, objectId);
            var tmp = _cache.GetString(key) ?? CreateTemporaryFile();

            _cache.SetString(key, tmp, _cacheEntryOptions);

            return File.OpenWrite(tmp);
        }

        public void FinalizeUpload(string repositoryId, string objectId, long expectedSize)
        {
            var key = StoreCaching.GetTempFileKey(repositoryId, objectId);
            var path = _cache.GetString(key);
            if (path is null)
            {
                throw new ObjectNotFoundError();
            }
            _cache.Remove(key);
            
            var tmp = new FileInfo(path);
            if (!tmp.Exists)
                throw new ObjectNotFoundError();
            if (tmp.Length != expectedSize)
            {
                tmp.Delete();
                throw new BadRequestError();
            }
            
            var file = new FileInfo(GetObjectPath(repositoryId, objectId));

            file.Directory?.Create();
            tmp.MoveTo(file.FullName, true);
        }
        
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var tmp in _tmpDirectory.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
                    if (tmp.LastAccessTime.Add(_options.Expiration) < DateTime.Now)
                    {
                        tmp.Delete();
                    }

                await Task.Delay(_options.ScanInterval, cancellationToken);
            }
        }
    }
}