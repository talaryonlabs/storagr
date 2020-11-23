using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Data.Entities;

namespace Storagr
{
    public interface ILockService
    {
        Task<LockEntity> Create(string repositoryId, string path);
        
        Task<LockEntity> Get(string repositoryId, string lockId);
        Task<LockEntity> GetByPath(string repositoryId, string path);
        Task<IEnumerable<LockEntity>> GetAll(string repositoryId) => 
            GetAll(repositoryId, -1, null, null, null);
        Task<IEnumerable<LockEntity>> GetAll(string repositoryId, int limit, string cursor) =>
            GetAll(repositoryId, limit, cursor, null, null);
        Task<IEnumerable<LockEntity>> GetAll(string repsitoryId, int limit, string cursor, string lockIdPattern, string pathPattern);

        Task Delete(string repositoryId, string lockId);
        Task DeleteAll(string repositoryId);
    }
}