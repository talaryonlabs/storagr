using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using Storagr.Server.Data.Entities;
using Storagr.Server.Services;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server
{
    public interface IStoragrService
    {
        IStoragrServiceAuthorization Authorization();
        IStoragrLogList Logs();

        IStoragrServiceRepository Repository(string repositoryIdOrName);
        IStoragrServiceRepositories Repositories();

        IStoragrServiceUser User(string userIdOrName);
        IStoragrServiceUsers Users();
    }

    public interface IStoragrServiceUser : 
        IStoragrRunner<UserEntity>,
        IStoragrExistable,
        IStoragrCreatable<UserEntity, IUserParams>,
        IStoragrUpdatable<UserEntity, IUserParams>,
        IStoragrDeletable<UserEntity>
    {
        
    }

    public interface IStoragrServiceUsers :
        IStoragrEnumerable<UserEntity, IUserParams>,
        IStoragrCountable
    {
        
    }

    public interface IStoragrServiceAuthorization
    {
        IStoragrRunner<UserEntity> GetAuthenticatedUser();
        IStoragrRunner<string> GetAuthenticatedToken();
        IStoragrServiceAuthorizationAuthentication Authenticate();
    }

    public interface IStoragrServiceAuthorizationAuthentication
    {
        IStoragrRunner<IStoragrServiceAuthorizationResult> With(string username, string password);
        IStoragrRunner<IStoragrServiceAuthorizationResult> With(string token);
    }

    public interface IStoragrServiceAuthorizationResult
    {
        UserEntity User { get; }
        string Token { get; }
    }

    public interface IStoragrServiceRepository :
        IStoragrRunner<RepositoryEntity>,
        IStoragrExistable,
        IStoragrCreatable<RepositoryEntity, IRepositoryParams>,
        IStoragrUpdatable<RepositoryEntity, IRepositoryParams>,
        IStoragrDeletable<RepositoryEntity>
    {
        IStoragrServiceObject Object(string objectId);
        IStoragrServiceObjects Objects();

        IStoragrServiceLock Lock(string lockIdOrPath);
        IStoragrServiceLocks Locks();
        
        IStoragrRunner GrantAccess(string userId, RepositoryAccessType accessType);
        IStoragrRunner RevokeAccess(string userId);
        IStoragrRunner<bool> HasAccess(string userId, RepositoryAccessType accessType);
    }

    public interface IStoragrServiceRepositories :
        IStoragrEnumerable<RepositoryEntity, IRepositoryParams>,
        IStoragrCountable
    {
        
    }

    public interface IStoragrServiceObject :
        IStoragrRunner<ObjectEntity>,
        IStoragrExistable,
        IStoragrCreatable<ObjectEntity, IObjectParams>,
        IStoragrDeletable<ObjectEntity>
    {
    }

    public interface IStoragrServiceObjects :
        IStoragrEnumerable<ObjectEntity, IObjectParams>,
        IStoragrCountable
    {
    }
    
    public interface IStoragrServiceLock :
        IStoragrRunner<LockEntity>,
        IStoragrExistable,
        IStoragrCreatable<LockEntity>,
        IStoragrDeletable<LockEntity>
    {
    }

    public interface IStoragrServiceLocks :
        IStoragrEnumerable<LockEntity, ILockParams>,
        IStoragrCountable
    {
    }
}