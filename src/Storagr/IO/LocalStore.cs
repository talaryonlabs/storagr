using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Storagr.Services;

namespace Storagr.IO
{
    public sealed class LocalStoreOptions : StoragrOptions<LocalStoreOptions>
    {
        public string RootPath { get; set; }
    }

    public static class LocalStoreExtension
    {
        public static IServiceCollection AddLocalStore(this IServiceCollection services, Action<LocalStoreOptions> configureOptions)
        {
            return services
                .AddOptions()
                .Configure(configureOptions)
                .AddSingleton<LocalStore>()
                .AddSingleton<IStoreAdapter>(x => x.GetRequiredService<LocalStore>());
        }
    }

    public sealed class LocalStoreObject : StoreObject
    {
        public string Name;
        public string Path;
        public Stream GetStream()
        {
            if(!File.Exists(Path))
                throw new FileNotFoundException();
            
            return File.OpenRead(Path);
        }
    }

    public sealed class LocalStore : IStoreAdapter, IDisposable
    {
        private readonly IUserService _userService; 
        private readonly LocalStoreOptions _options;

        private readonly DirectoryInfo
            _rootDirectory,
            _tmpDirectory;

        private bool _isDisposed;

        public LocalStore(IOptions<LocalStoreOptions> optionsAccessor, IUserService userService)
        {
            if (optionsAccessor == null) throw new ArgumentNullException(nameof(optionsAccessor));

            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _options = optionsAccessor.Value;
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
            return Path.Combine(new[]
            {
                _rootDirectory.FullName,
                repositoryId
            });
        }

        public Task<StoreRepository> GetRepository(string repositoryId)
        {
            var path = GetRepositoryPath(repositoryId);
            var directory = new DirectoryInfo(path);
            var repository = new StoreRepository()
            {
                RepositoryId = directory.Name,
                UsedSpace = directory.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length)
            };
            return Task.FromResult(repository);
        }

        public Task<IEnumerable<StoreRepository>> ListRepositories()
        {
            var repositories = _rootDirectory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).Select(d => new StoreRepository()
            {
                RepositoryId = d.Name,
                UsedSpace = d.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length)
            });
            return Task.FromResult(repositories);
        }

        public Task DeleteRepository(string repositoryId)
        {
            if (string.IsNullOrEmpty(repositoryId)) throw new ArgumentNullException(nameof(repositoryId));

            var directory = Path.Combine(_rootDirectory.FullName, repositoryId);
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);

            return Task.CompletedTask;
        }

        public Task<StoreObject> GetObject(string repositoryId, string objectId)
        {
            if (string.IsNullOrEmpty(repositoryId)) throw new ArgumentNullException(nameof(repositoryId));
            if (string.IsNullOrEmpty(objectId)) throw new ArgumentNullException(nameof(objectId));
            
            var path = GetObjectPath(repositoryId, objectId);
            var file = new FileInfo(path);

            if (!file.Exists)
                return Task.FromResult<StoreObject>(default);

            var storeObject = new LocalStoreObject
            {
                RepositoryId = repositoryId,
                ObjectId = objectId,
                Size = file.Length,
                Name = file.Name,
                Path = file.FullName
            };
            return Task.FromResult((StoreObject)storeObject);
        }

        public Task<IEnumerable<StoreObject>> ListObjects(string repositoryId)
        {
            if (string.IsNullOrEmpty(repositoryId)) throw new ArgumentNullException(nameof(repositoryId));

            var repositoryPath = GetRepositoryPath(repositoryId);
            var repositoryDirectory = new DirectoryInfo(repositoryPath);
            
            if (!repositoryDirectory.Exists) 
                throw new DirectoryNotFoundException();

            var list = repositoryDirectory
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Select(file => new LocalStoreObject()
                {
                    RepositoryId = repositoryId,
                    ObjectId = file.Name,
                    Path = file.FullName,
                    Name = file.Name,
                    Size = file.Length
                }).ToList();
            
            return Task.FromResult((IEnumerable<StoreObject>)list);
        }

        public Task DeleteObject(string repositoryId, string objectId)
        {
            if (string.IsNullOrEmpty(repositoryId)) throw new ArgumentNullException(nameof(repositoryId));
            if (string.IsNullOrEmpty(objectId)) throw new ArgumentNullException(nameof(objectId));

            var path = GetObjectPath(repositoryId, objectId);
            var file = new FileInfo(path);

            if (file.Exists)
                file.Delete();

            if (Directory.GetFiles(file?.Directory?.FullName).Length == 0)
                Directory.Delete(file?.Directory?.FullName);

            if (Directory.GetFiles(file?.Directory?.Parent?.FullName).Length == 0)
                Directory.Delete(file?.Directory?.Parent?.FullName);

            return Task.CompletedTask;
        }

        public async Task<StoreRequest> NewDownloadRequest(string repositoryId, string objectId)
        {
            var obj = await GetObject(repositoryId, objectId);
            var token = await _userService.GetAuthenticatedUserToken();
            
            if (obj != null)
            {
                return new StoreRequest
                {
                    Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                    ExpiresAt = default,
                    ExpiresIn = 0,
                    Url = $"{repositoryId}/store/{objectId}"
                };
            }
            return null;
        }

        public async Task<(StoreRequest, StoreRequest)> NewUploadRequest(string repositoryId, string objectId)
        {
            var obj = await GetObject(repositoryId, objectId);
            if (obj != null) 
                return (null, null);
            
            var token = await _userService.GetAuthenticatedUserToken();
            var uploadRequest = new StoreRequest
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 0,
                Url = $"{repositoryId}/store/{objectId}"
            };
            var verifyRequest = new StoreRequest
            {
                Header = new Dictionary<string, string>() {{"Authorization", $"Bearer {token}"}},
                ExpiresAt = default,
                ExpiresIn = 0,
                Url = $"{repositoryId}/store/{objectId}"
            };
            return (uploadRequest, verifyRequest);

        }
        
        public FileInfo CreateTemporaryFile()
        {
            var name = Path.GetRandomFileName();
            var path = Path.Combine(_tmpDirectory.FullName, name);

            return new FileInfo(path);
        }

        public FileInfo GetTemporaryFile(string name)
        {
            var path = Path.Combine(_tmpDirectory.FullName, name);
            return !File.Exists(path) ? null : new FileInfo(path);
        }

        public void Save(FileInfo tempFile, string repositoryId, string objectId)
        {
            if (tempFile == null) throw new ArgumentNullException(nameof(tempFile));
            if (!tempFile.Exists) throw new FileNotFoundException();
            if (string.IsNullOrEmpty(repositoryId)) throw new ArgumentNullException(nameof(repositoryId));
            if (string.IsNullOrEmpty(objectId)) throw new ArgumentNullException(nameof(objectId));

            var path = GetObjectPath(repositoryId, objectId);
            var file = new FileInfo(path);

            file.Directory?.Create();
            tempFile.MoveTo(path, true);
        }

        public void Dispose()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_isDisposed || !(_isDisposed = true))
                return;
        }
    }
}