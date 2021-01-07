using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Data.Entities;
using Storagr.Shared.Data;

namespace Storagr
{
    public interface IObjectService
    {
        Task<int> Count(string repositoryId, CancellationToken cancellationToken = default);
        Task<bool> Exists(string repositoryId, string objectId, CancellationToken cancellationToken = default);

        Task<ObjectEntity> Get(string repositoryId, string objectId, CancellationToken cancellationToken = default);

        Task<IEnumerable<ObjectEntity>> GetMany(string repositoryId, IEnumerable<string> objectIds,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ObjectEntity>> GetAll(string repositoryId, CancellationToken cancellationToken = default);

        Task<ObjectEntity> Add(string repositoryId, ObjectEntity newObject, CancellationToken cancellationToken = default);
        Task<ObjectEntity> Delete(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ObjectEntity>> DeleteAll(string repositoryId, CancellationToken cancellationToken = default);

        Task<StoragrAction> NewVerifyAction(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        Task<StoragrAction> NewUploadAction(string repositoryId, string objectId, CancellationToken cancellationToken = default);

        Task<StoragrAction> NewDownloadAction(string repositoryId, string objectId,
            CancellationToken cancellationToken = default);

        
    }
}