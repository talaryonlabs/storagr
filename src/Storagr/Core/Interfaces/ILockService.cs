using System.Threading.Tasks;
using Storagr.Data.Entities;
using Storagr.Shared.Data;

namespace Storagr
{
    public struct LockSearchArgs
    {
        public string LockId;
        public string Path;
        
        public static implicit operator LockSearchArgs(StoragrLockListArgs args) =>
            new LockSearchArgs()
            {
                LockId = args.LockId,
                Path = args.Path
            };
    }
    
    public interface ILockService : IContainerService<LockEntity, LockSearchArgs>
    {
        Task<bool> ExistsByPath(string repositoryId, string path);
        Task<LockEntity> GetByPath(string repositoryId, string path);
    }
}