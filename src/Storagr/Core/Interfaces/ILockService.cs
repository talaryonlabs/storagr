using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Data.Entities;

namespace Storagr
{
    public interface ILockService
    {
        Task<int> Count(string repositoryId, CancellationToken cancellationToken = default);
        Task<bool> Exists(string repositoryId, string lockId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPath(string repositoryId, string lockedPath, CancellationToken cancellationToken = default);

        Task<LockEntity> Get(string repositoryId, string lockId, CancellationToken cancellationToken = default);
        Task<LockEntity> GetByPath(string repositoryId, string lockedPath, CancellationToken cancellationToken = default);

        Task<IEnumerable<LockEntity>> GetMany(string repositoryId, string lockId = null, string lockedPath = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<LockEntity>> GetAll(string repositoryId, CancellationToken cancellationToken = default);

        Task<LockEntity> Lock(string repositoryId, string path, CancellationToken cancellationToken = default);
        Task<LockEntity> Unlock(string repositoryId, string lockId, CancellationToken cancellationToken = default);
        Task<IEnumerable<LockEntity>> UnlockAll(string repositoryId, CancellationToken cancellationToken = default);
    }
}