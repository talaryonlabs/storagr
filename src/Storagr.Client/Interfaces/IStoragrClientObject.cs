using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrClientObject :
        IStoragrRunner<StoragrObject>,
        IStoragrExistable,
        IStoragrDeletable<StoragrObject>
    {
    }
    
    public interface IStoragrClientObjectList :
        IStoragrListable<StoragrObject, IStoragrObjectParams>
    {
    }
}