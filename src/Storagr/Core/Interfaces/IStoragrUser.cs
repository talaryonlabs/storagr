using Storagr.Data;

namespace Storagr
{
    public interface IStoragrUser :
        IStoragrExistable,
        IStoragrRunner<StoragrUser>,
        IStoragrCreatable<StoragrUser, IStoragrUserParams>,
        IStoragrUpdatable<StoragrUser, IStoragrUserParams>,
        IStoragrDeletable<StoragrUser>
    {
        
    }

    public interface IStoragrUserList :
        IStoragrCountable,
        IStoragrEnumerable<StoragrUser, IStoragrUserParams>
    {
        
    }

    public interface IStoragrUserProvider :
        IStoragrUserProvider<IStoragrUser, IStoragrUserList>
    {
    }

    public interface IStoragrUserProvider<out TItem, out TList>
        where TItem : IStoragrUser
        where TList : IStoragrUserList
    {
        TItem User(string userIdOrName);
        TList Users();
    }
}