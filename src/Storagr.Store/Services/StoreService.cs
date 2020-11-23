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
        private readonly StoreServiceOptions _options;
        private readonly DirectoryInfo _rootDirectory;
        private readonly DirectoryInfo _tmpDirectory;

        public StoreService(IOptions<StoreServiceOptions> optionsAccessor, IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(IDistributedCache));
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(StoreServiceOptions));
            _rootDirectory = new DirectoryInfo(_options.RootPath);
            _tmpDirectory = new DirectoryInfo(Path.Combine(_options.RootPath, ".tmp"));

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
                throw new RepositoryNotFoundException();

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
                throw new ObjectNotFoundException();

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
                throw new RepositoryNotFoundException();
            
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
                throw new RepositoryNotFoundException();
            
            Directory.Delete(path, true);
        }
        public void Delete(string repositoryId, string objectId)
        {
            var path = GetObjectPath(repositoryId, objectId);
            if (!File.Exists(path))
                throw new ObjectNotFoundException();
            
            File.Delete(path);
        }

        public Stream GetDownloadStream(string repositoryId, string objectId)
        {
            var path = GetObjectPath(repositoryId, objectId);
            if (!File.Exists(path))
                throw new ObjectNotFoundException();

            return File.OpenRead(path);
        }

        public Stream GetUploadStream(string repositoryId, string objectId)
        {
            var path = GetObjectPath(repositoryId, objectId);
            if (File.Exists(path))
            {
                throw new ObjectExistsException();
            }
            var key = $"STORAGR:STORE:TMP:{repositoryId}:{objectId}";
            var tmp = _cache.GetString(key) ?? CreateTemporaryFile();
            
            _cache.SetString(key, tmp, new DistributedCacheEntryOptions().SetAbsoluteExpiration(_options.Expiration));

            return File.OpenWrite(tmp);
        }

        public bool FinalizeUpload(string repositoryId, string objectId, long expectedSize)
        {
            var key = $"STORAGR:STORE:TMP:{repositoryId}:{objectId}";
            var path = _cache.GetString(key);
            if (path == null)
            {
                return false;
            }
            _cache.Remove(key);
            
            var tmp = new FileInfo(path);
            if (!tmp.Exists)
                return false;
            if (tmp.Length != expectedSize)
            {
                tmp.Delete();
                return false;
            }
            
            var file = new FileInfo(GetObjectPath(repositoryId, objectId));

            file.Directory?.Create();
            tmp.MoveTo(file.FullName, true);

            return true;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var tmp in _tmpDirectory.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
                    if (tmp.LastAccessTime.Add(_options.Expiration) < DateTime.Now)
                    {
                        tmp.Delete();
                    }

                await Task.Delay(_options.ScanInterval, stoppingToken);
            }
        }
    }
}