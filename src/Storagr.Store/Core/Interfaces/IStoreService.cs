using System.Collections.Generic;
using System.IO;
using Storagr.Shared.Data;

namespace Storagr.Store
{
    public interface IStoreService
    {
        int BufferSize { get; }

        bool Exists(string repositoryId);
        bool Exists(string repositoryId, string objectId);

        StoreRepository Get(string repositoryId);
        StoreObject Get(string repositoryId, string objectId);
        
        IEnumerable<StoreRepository> List();
        IEnumerable<StoreObject> List(string repositoryId);
        
        void Delete(string repositoryId);
        void Delete(string repositoryId, string objectId);
        
        Stream GetDownloadStream(string repositoryId, string objectId);
        Stream GetUploadStream(string repositoryId, string objectId);
        void FinalizeUpload(string repositoryId, string objectId, long expectedSize);
    }
}