using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Store
{
    public interface ITransferService
    {
        StoreTransferObject AddRequest(string repositoryId, string objectId);
        Task<StoreTransferObject> AddRequestAsync(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        
        StoreTransferObject GetRequest(string transferId);
        Task<StoreTransferObject> GetRequestAsync(string transferId, CancellationToken cancellationToken = default);
    }
}