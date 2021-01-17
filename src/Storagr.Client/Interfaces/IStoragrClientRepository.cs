using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrRepositoryParams
    {
        IStoragrRepositoryParams Id(string repositoryId);
        IStoragrRepositoryParams Name(string name);
        IStoragrRepositoryParams Owner(string owner);
        IStoragrRepositoryParams SizeLimit(ulong sizeLimit);
    }

    public interface IStoragrClientRepository : 
        IStoragrClientRunner<StoragrRepository>,
        IStoragrClientCreatable<StoragrRepository, IStoragrRepositoryParams>,
        IStoragrClientUpdatable<StoragrRepository, IStoragrRepositoryParams>,
        IStoragrClientDeletable<StoragrRepository>
    {
        IStoragrClientObject Object(string objectId);
        IStoragrClientObjectList Objects();

        IStoragrClientLock Lock(string lockIdOrPath);
        IStoragrClientLockList Locks();
    }

    public interface IStoragrClientRepositoryList :
        IStoragrClientList<StoragrRepository, IStoragrRepositoryParams>
    {

    }
}