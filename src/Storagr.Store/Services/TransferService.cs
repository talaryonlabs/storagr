using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Storagr;
using Storagr.Shared.Data;

namespace Storagr.Store.Services
{
    public class TransferService : ITransferService
    {
        private readonly IDistributedCache _cache;

        public TransferService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public StoreTransferObject AddRequest(string repositoryId, string objectId)
        {
            var obj = new StoreTransferObject()
            {
                TransferId = StoragrHelper.UUID(),
                ObjectId = objectId,
                RepositoryId = repositoryId
            };
            _cache.Set(
                StoreCaching.GetTransferRequestKey(obj.TransferId),
                StoragrHelper.SerializeObject(obj)
            );

            return obj;
        }

        public async Task<StoreTransferObject> AddRequestAsync(string repositoryId, string objectId, CancellationToken cancellationToken)
        {
            var obj = new StoreTransferObject()
            {
                TransferId = StoragrHelper.UUID(),
                ObjectId = objectId,
                RepositoryId = repositoryId
            };
            await _cache.SetAsync(
                StoreCaching.GetTransferRequestKey(obj.TransferId),
                StoragrHelper.SerializeObject(obj),
                cancellationToken
            );

            return obj;
        }

        public StoreTransferObject GetRequest(string transferId) =>
            StoragrHelper.DeserializeObject<StoreTransferObject>(
                _cache.Get(
                    StoreCaching.GetTransferRequestKey(transferId)
                )
            );

        public async Task<StoreTransferObject> GetRequestAsync(string transferId, CancellationToken cancellationToken) =>
            StoragrHelper.DeserializeObject<StoreTransferObject>(
                await _cache.GetAsync(
                    StoreCaching.GetTransferRequestKey(transferId),
                    cancellationToken
                )
            );
    }
}