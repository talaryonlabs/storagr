using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Storagr.Shared.Data;

namespace Storagr.Store.Services
{
    public partial class StoreService : IStoreService
    {
        public int BufferSize => _config.BufferSize;

        public long UsedSpace => _rootDirectory
            .GetFiles("*", SearchOption.AllDirectories)
            .Sum(file => file.Length);

        public long AvailableSpace => new DriveInfo(_rootDirectory.FullName).AvailableFreeSpace;

        private readonly DirectoryInfo _rootDirectory;
        private readonly StoreConfig _config;

        public StoreService(IOptions<StoreConfig> options)
        {
            _config = options.Value;
            _rootDirectory = new DirectoryInfo(_config.RootPath);

            if (!_rootDirectory.Exists) throw new DirectoryNotFoundException();
        }

        public IStoreObject Object(string objectId) =>
            new StoreServiceObject(
                new FileInfo(_rootDirectory.CombineWith($"{objectId[..2]}/{objectId[2..4]}/{objectId}").FullName)
            );

        public IEnumerable<StoreObject> Objects() => _rootDirectory
            .GetFiles("*", SearchOption.AllDirectories)
            .Select(file => new StoreObject()
            {
                ObjectId = file.Name,
                Size = file.Length
            });
    }
}