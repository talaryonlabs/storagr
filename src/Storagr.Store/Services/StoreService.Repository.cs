using System.Collections.Generic;
using System.IO;
using System.Linq;
using Storagr;
using Storagr.Data;

namespace Storagr.Store.Services
{
    public partial class StoreService
    {
        private class StoreServiceRepository : IStoreRepository
        {
            public string Id
            {
                get
                {
                    if (!Exists())
                        throw new RepositoryNotFoundError();

                    return _directory.Name;
                }
            }

            public string Name => (MetaInfo ?? throw new RepositoryNotFoundError()).Name;

            public ulong Size => (ulong) _directory
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(f => f.Name != "metainfo")
                .Sum(f => f.Length);

            private StoreMetaInfo MetaInfo => _metaInfo ??= StoreMetaInfo.FromFile(Path.Combine(_directory.FullName, "metainfo"));

            private readonly DirectoryInfo _directory;
            private StoreMetaInfo _metaInfo;

            public StoreServiceRepository(DirectoryInfo repositoryDirectory)
            {
                _directory = repositoryDirectory;
            }

            private DirectoryInfo GetObjectDirectory(string objectId) =>
                new(Path.Combine(
                    _directory.FullName,
                    objectId.Substring(0, 2),
                    objectId.Substring(2, 2)
                ));

            private FileInfo GetObjectFile(string objectId) =>
                new(Path.Combine(
                    GetObjectDirectory(objectId).FullName,
                    objectId
                ));

            public bool Exists() => _directory.Exists;

            public IStoreRepository CreateIfNotExists()
            {
                if (!Exists())
                    _directory.Create();

                return this;
            }

            public IStoreRepository SetName(string name)
            {
                StoreMetaInfo.ToFile(Path.Combine(_directory.FullName, "metainfo"), new StoreMetaInfo()
                {
                    Name = name
                });

                return this;
            }

            public IStoreObject Object(string objectId)
            {
                var file = GetObjectFile(objectId);
                if (!(file.Directory!.Exists))
                    Directory.CreateDirectory(file.DirectoryName!);

                return new StoreServiceObject(file);
            }


            public IEnumerable<IStoreObject> Objects() =>
                _directory
                    .EnumerateFiles("*", SearchOption.AllDirectories)
                    .Where(file => file.Name != "metainfo")
                    .Select(file =>
                        new StoreServiceObject(file)
                    );

            public void Delete()
            {
                if (!Exists())
                    throw new RepositoryNotFoundError();

                _directory.Delete(true);
            }

            public StoreRepository Model()
            {
                if (!Exists())
                    throw new RepositoryNotFoundError();

                return new StoreRepository()
                {
                    Id = Id,
                    Name = Name,
                    Size = Size
                };
            }
        }
    }
}