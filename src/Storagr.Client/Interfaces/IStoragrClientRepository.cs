using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrClientRepository :
        IStoragrRunner<StoragrRepository>,
        IStoragrExistable,
        IStoragrCreatable<StoragrRepository, IStoragrRepositoryParams>,
        IStoragrUpdatable<StoragrRepository, IStoragrRepositoryParams>,
        IStoragrDeletable<StoragrRepository>
    {
        IStoragrClientObject Object(string objectId);
        IStoragrClientObjectList Objects();

        IStoragrClientLock Lock(string lockIdOrPath);
        IStoragrClientLockList Locks();
    }

    public interface IStoragrClientRepositoryList :
        IStoragrListable<StoragrRepository, IStoragrRepositoryParams>
    {

    }
}