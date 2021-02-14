using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrClientUser :
        IStoragrRunner<StoragrUser>,
        IStoragrExistable,
        IStoragrCreatable<StoragrUser, IStoragrUserParams>,
        IStoragrUpdatable<StoragrUser, IStoragrUserParams>,
        IStoragrDeletable<StoragrUser>
    {
        
    }
    
    public interface IStoragrClientUserList :
        IStoragrListable<StoragrUser, IStoragrUserParams>
    {

    }
}