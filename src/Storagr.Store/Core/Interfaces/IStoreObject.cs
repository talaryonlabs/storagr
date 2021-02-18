using System.IO;

namespace Storagr.Store
{
    public interface IStoreObject
    {
        bool Exists { get; }
        string Id { get; }
        long Size { get; }

        Stream GetStream();
        void Delete();
    }
}