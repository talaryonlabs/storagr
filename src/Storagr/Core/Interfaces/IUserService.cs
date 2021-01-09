using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Data.Entities;

namespace Storagr
{
    public interface IUserService
    {
        Task<int> Count(string username = null, CancellationToken cancellationToken = default);
        Task<bool> Exists(string userIdOrName, CancellationToken cancellationToken = default);
        
        Task<UserEntity> Get(string userIdOrName, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserEntity>> GetMany(string username = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserEntity>> GetAll(CancellationToken cancellationToken = default);

        Task<UserEntity> Create(UserEntity newUser, string newPassword, CancellationToken cancellationToken = default);
        Task<UserEntity> Modify(UserEntity updatedUser, string newPassword = null, CancellationToken cancellationToken = default);
        Task<UserEntity> Delete(string userId, CancellationToken cancellationToken = default);

        Task<UserEntity> GetAuthenticatedUser(CancellationToken cancellationToken = default);
        Task<string> GetAuthenticatedToken(CancellationToken cancellationToken = default);
        Task<UserEntity> Authenticate(string username, string password, CancellationToken cancellationToken = default);
    }
}