using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Data.Entities;
using Storagr.Shared.Data;

namespace Storagr
{
    public interface IObjectService : IContainerService<ObjectEntity>
    {
        Task<StoragrAction> NewVerifyAction(string repositoryId, string objectId);
        Task<StoragrAction> NewUploadAction(string repositoryId, string objectId);
        Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId);
    }
}