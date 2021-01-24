using Storagr.Server.Data.Entities;

namespace Storagr.Server.Services
{
    public partial class StoragrService : IStoragrService
    {
        private IDatabaseAdapter Database { get; }

        public StoragrService(IDatabaseAdapter database)
        {
            Database = database;
        }
        
        IUserServiceItem IUserService.User(string userIdOrName)
        {
            throw new System.NotImplementedException();
        }

        IUserServiceList IUserService.Users()
        {
            throw new System.NotImplementedException();
        }

        IStoragrRunner<UserEntity> IUserService.GetAuthenticatedUser()
        {
            throw new System.NotImplementedException();
        }

        IStoragrRunner<string> IUserService.GetAuthenticatedToken()
        {
            throw new System.NotImplementedException();
        }

        IStoragrRunner<UserEntity> IUserService.Authenticate(string username, string password)
        {
            throw new System.NotImplementedException();
        }

        IRepositoryServiceItem IRepositoryService.Repository(string repositoryIdOrName)
        {
            return new StoragrRepositoryService(this, repositoryIdOrName);
        }

        IRepositoryServiceList IRepositoryService.Repositories()
        {
            throw new System.NotImplementedException();
        }
    }
}