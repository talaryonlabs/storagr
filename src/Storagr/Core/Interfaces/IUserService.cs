using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Data.Entities;
using Storagr.Services;

namespace Storagr
{
    public interface IUserService
    {
        Task<UserEntity> GetAuthenticatedUser();
        Task<string> GetAuthenticatedUserToken();
        
        Task<UserEntity> Authenticate(string username, string password);
        // Task<(string, UserEntity)> Authenticate(string token);

        Task<UserEntity> CreateOrUpdate(string authAdapater, string authId, string username, string mail, string role);
        Task Modify(UserEntity entity);
        Task Delete(string userId);

        Task<UserEntity> Get(string userId);
        Task<IEnumerable<UserEntity>> GetAll();
    }
}