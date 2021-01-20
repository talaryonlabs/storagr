using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Data.Entities;

namespace Storagr
{
    public interface IRepositoryService
    {
        Task<bool> Exists(string repositoryIdOrName, CancellationToken cancellationToken = default);
        Task<int> Count(string repositoryIdOrName = null, string ownerId = null, CancellationToken cancellationToken = default);

        Task<RepositoryEntity> Get(string repositoryIdOrName, CancellationToken cancellationToken = default);

        Task<IEnumerable<RepositoryEntity>> GetMany(string repositoryIdOrName = null, string ownerId = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<RepositoryEntity>> GetAll(CancellationToken cancellationToken = default);

        Task<RepositoryEntity> Create(RepositoryEntity newRepository, CancellationToken cancellationToken = default);
        Task<RepositoryEntity> Update(RepositoryEntity updatedRepository, CancellationToken cancellationToken = default);
        Task<RepositoryEntity> Delete(string repositoryIdOrName, CancellationToken cancellationToken = default);

        Task GrantAccess(string repositoryIdOrName, string userId, RepositoryAccessType accessType,
            CancellationToken cancellationToken = default);

        Task RevokeAccess(string repositoryIdOrName, string userId, CancellationToken cancellationToken = default);

        Task<bool> HasAccess(string repositoryIdOrName, string userId, RepositoryAccessType accessType,
            CancellationToken cancellationToken = default);
    }
}