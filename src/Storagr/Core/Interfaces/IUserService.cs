using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.UserSecrets;
using Storagr.Data.Entities;
using Storagr.Services;
using Storagr.Shared.Data;

namespace Storagr
{
    public struct UserSearchArgs
    {
        public string Username;
        
        public static implicit operator UserSearchArgs(StoragrUserListArgs args) =>
            new UserSearchArgs()
            {
                Username = args.Username
            };
    }

    public interface IUserService : IEntityService<UserEntity, UserSearchArgs>
    {
        Task<bool> ExistsByUsername(string username);
        
        Task<UserEntity> GetAuthenticatedUser();
        Task<string> GetAuthenticatedToken();

        Task<UserEntity> Authenticate(string username, string password);
        
        Task Modify(UserEntity entity, string newPassword);
    }
}