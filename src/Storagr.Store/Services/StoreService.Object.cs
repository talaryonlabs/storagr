using System.IO;

namespace Storagr.Store.Services
{
    public partial class StoreService
    {
        private class StoreServiceObject : IStoreObject
        {
            public bool Exists => _file.Exists;
            public string Id => _file.Name;
            public long Size => _file.Length;
            
            
            private readonly FileInfo _file;

            public StoreServiceObject(FileInfo file)
            {
                _file = file;
            }

            public void Delete() => _file.Delete();

            public Stream GetStream() => _file.OpenWrite();
        }
    }
}