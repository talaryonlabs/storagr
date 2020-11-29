using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Data.Entities;
using Storagr.Services;

namespace Storagr
{
    public enum RepositoryAccessType
    {
        Read,
        Write
    }
    
    public interface IUserService
    {
        Task<UserEntity> GetAuthenticatedUser();
        Task<string> GetAuthenticatedToken();
        Task<bool> HasAccess(RepositoryEntity repository, RepositoryAccessType accessType);

        Task<UserEntity> Authenticate(string username, string password);
        
        Task<bool> Exists(string username);
        
        Task<UserEntity> Create(string username, string password, bool isAdmin);
        Task Modify(UserEntity entity, string newPassword);
        Task Delete(string userId);

        Task<UserEntity> Get(string userId);
        Task<IEnumerable<UserEntity>> GetAll();
    }
}