using System.Threading.Tasks;
using Storagr.Data.Entities;
using Storagr.Shared.Data;

namespace Storagr
{
    public struct RepositorySearchArgs
    {
        public string RepositoryId;

        public static implicit operator RepositorySearchArgs(StoragrRepositoryListArgs args) =>
            new RepositorySearchArgs()
            {
                RepositoryId = args.Id
            };
    }

    public interface IRepositoryService : IEntityService<RepositoryEntity, RepositorySearchArgs>
    {
        Task GrantAccess(string repositoryId, string userId, RepositoryAccessType accessType);
        Task RevokeAccess(string repositoryId, string userId);
        Task<bool> HasAccess(string repositoryId, string userId, RepositoryAccessType accessType);
    }
}