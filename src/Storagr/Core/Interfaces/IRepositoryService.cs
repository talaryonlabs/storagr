using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Data.Entities;

namespace Storagr
{
    public interface IRepositoryService
    {
        Task<bool> Exists(string repositoryId, CancellationToken cancellationToken = default);
        Task<int> Count(string repositoryId = null, string ownerId = null, CancellationToken cancellationToken = default);

        Task<RepositoryEntity> Get(string repositoryId, CancellationToken cancellationToken = default);

        Task<IEnumerable<RepositoryEntity>> GetMany(string repositoryId = null, string ownerId = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<RepositoryEntity>> GetAll(CancellationToken cancellationToken = default);

        Task<RepositoryEntity> Create(RepositoryEntity newRepository, CancellationToken cancellationToken = default);
        Task<RepositoryEntity> Delete(string entityId, CancellationToken cancellationToken = default);

        Task GrantAccess(string repositoryId, string userId, RepositoryAccessType accessType,
            CancellationToken cancellationToken = default);

        Task RevokeAccess(string repositoryId, string userId, CancellationToken cancellationToken = default);

        Task<bool> HasAccess(string repositoryId, string userId, RepositoryAccessType accessType,
            CancellationToken cancellationToken = default);
    }
}