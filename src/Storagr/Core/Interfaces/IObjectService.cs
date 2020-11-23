using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Data.Entities;
using Storagr.Shared.Data;

namespace Storagr
{
    public interface IObjectService
    {
        Task<ObjectEntity> Create(string repositoryId, string objectId, long size);

        Task<RepositoryEntity> Get(string repositoryId);
        Task<ObjectEntity> Get(string repositoryId, string objectId);
        Task<IEnumerable<ObjectEntity>> GetMany(string repositoryId, params string[] objectIds);
        Task<IEnumerable<ObjectEntity>> GetAll(string repositoryId);

        Task Delete(string repositoryId);
        Task Delete(string repositoryId, string objectId);

        Task<StoragrAction> NewVerifyAction(string repositoryId, string objectId);
        Task<StoragrAction> NewUploadAction(string repositoryId, string objectId);
        Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId);
    }
}