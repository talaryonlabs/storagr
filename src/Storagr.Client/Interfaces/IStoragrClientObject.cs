using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrObjectParams
    {
        IStoragrObjectParams Id(string objectId);
    }
    
    public interface IStoragrClientObject :
        IStoragrClientRunner<StoragrObject>,
        IStoragrClientDeletable<StoragrObject>
    {

    }

    public interface IStoragrClientObjectList :
        IStoragrClientList<StoragrObject, IStoragrObjectParams>
    {

    }
}