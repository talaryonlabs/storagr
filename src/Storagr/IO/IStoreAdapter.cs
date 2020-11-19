using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storagr.IO
{
    public class StoreRequest
    {
        public string Url;
        public int ExpiresIn;
        public DateTime ExpiresAt;
        public IDictionary<string, string> Header;
    }

    public class StoreObject
    {
        public string ObjectId;
        public string RepositoryId;
        public long Size;
    }

    public class StoreRepository
    {
        public string RepositoryId;
        public long UsedSpace;
    }
    
    public interface IStoreAdapter
    {
        Task<StoreRepository> GetRepository(string repositoryId);
        Task<IEnumerable<StoreRepository>> ListRepositories();
        Task DeleteRepository(string repositoryId);
        
        Task<StoreObject> GetObject(string repositoryId, string objectId);
        Task<IEnumerable<StoreObject>> ListObjects(string repositoryId);
        Task DeleteObject(string repositoryId, string objectId);
        
        Task<StoreRequest> NewDownloadRequest(string repositoryId, string objectId);
        Task<(StoreRequest, StoreRequest)> NewUploadRequest(string repositoryId, string objectId);
    }
}