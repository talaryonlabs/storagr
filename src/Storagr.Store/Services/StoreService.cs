using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Storagr.Store.Services
{
    public partial class StoreService : IStoreService
    {
        public int BufferSize => _config.BufferSize;

        private readonly DirectoryInfo _rootDirectory;
        private readonly StoreConfig _config;

        public StoreService(IOptions<StoreConfig> options)
        {
            _config = options.Value;
            
            _rootDirectory = new DirectoryInfo(_config.RootPath);

            if(!_rootDirectory.Exists) throw new DirectoryNotFoundException();
        }

        public IStoreRepository Repository(string repositoryId) =>
            new StoreServiceRepository(_rootDirectory.CombineWith(repositoryId));

        public IEnumerable<IStoreRepository> Repositories() =>
            _rootDirectory
                .GetDirectories("*", SearchOption.TopDirectoryOnly)
                .Where(directory => directory.Name != ".tmp")
                .Select(directory => 
                    new StoreServiceRepository(directory)
                );
    }
}