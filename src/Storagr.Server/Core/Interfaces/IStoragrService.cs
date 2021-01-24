using Storagr.Data;
using Storagr.Server.Data.Entities;

namespace Storagr.Server
{
    public interface IStoragrService :
        IStoragrServiceRepository,
        IStoragrUserProvider
    {
        IStoragrRunner<UserEntity> GetAuthenticatedUser();
        IStoragrRunner<string> GetAuthenticatedToken();
        IStoragrRunner<UserEntity> Authenticate(string username, string password);
    }

    public interface IStoragrServiceRepository : 
        IStoragrRepository<IStoragrServiceObject, IStoragrObjectList>
    {
        IStoragrRunner GrantAccess(string userId, RepositoryAccessType accessType);
        IStoragrRunner RevokeAccess(string userId);
        IStoragrRunner<bool> HasAccess(string userId, RepositoryAccessType accessType);
    }
    
    public interface IStoragrServiceObject :
        IStoragrObject
    {
        IStoragrRunner<StoragrAction> NewVerifyAction();
        IStoragrRunner<StoragrAction> NewUploadAction();

        IStoragrRunner<StoragrAction> NewDownloadAction();
    }
}