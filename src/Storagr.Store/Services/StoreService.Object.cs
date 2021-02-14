using System.IO;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Services
{
    public partial class StoreService
    {
        private class StoreServiceObject : IStoreObject
        {
            public string Id
            {
                get
                {
                    if (!Exists())
                        throw new ObjectNotFoundError();

                    return _file.Name;
                }
            }

            public ulong Size
            {
                get
                {
                    if (!Exists())
                        throw new ObjectNotFoundError();

                    return (ulong) _file.Length;
                }
            }

            private readonly FileInfo _file;

            public StoreServiceObject(FileInfo objectFile)
            {
                _file = objectFile;
            }

            public bool Exists() => _file.Exists;

            public Stream GetDownloadStream()
            {
                if (!Exists())
                    throw new ObjectNotFoundError();

                return _file.OpenRead();
            }

            public Stream GetUploadStream()
            {
                if (Exists())
                    throw new ObjectAlreadyExistsError(null);

                return _file.OpenWrite();
            }

            public void Delete()
            {
                if (!Exists())
                    throw new ObjectNotFoundError();

                _file.Delete();
            }

            public StoreObject Model()
            {
                if (!Exists())
                    throw new ObjectNotFoundError();

                return new StoreObject()
                {
                    ObjectId = Id,
                    Size = Size
                };
            }
        }
    }
}