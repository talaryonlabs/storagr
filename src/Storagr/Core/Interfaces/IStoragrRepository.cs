using Storagr.Data;

namespace Storagr
{
    public interface IStoragrRepository :
        IStoragrExistable,
        IStoragrRunner<StoragrRepository>,
        IStoragrCreatable<StoragrRepository, IStoragrRepositoryParams>,
        IStoragrUpdatable<StoragrRepository, IStoragrRepositoryParams>,
        IStoragrDeletable<StoragrRepository>,
        
        IStoragrObjectProvider,
        IStoragrLockProvider
    {

    }

    public interface IStoragrRepository<out TObjectItem, out TObjectList> :
        IStoragrExistable,
        IStoragrRunner<StoragrRepository>,
        IStoragrCreatable<StoragrRepository, IStoragrRepositoryParams>,
        IStoragrUpdatable<StoragrRepository, IStoragrRepositoryParams>,
        IStoragrDeletable<StoragrRepository>,

        IStoragrObjectProvider<TObjectItem, TObjectList> 
        where TObjectItem : IStoragrObject 
        where TObjectList : IStoragrObjectList
    {

    }

    public interface IStoragrRepositoryList :
        IStoragrCountable,
        IStoragrEnumerable<StoragrRepository, IStoragrRepositoryParams>
    {

    }

    public interface IStoragrRepositoryProvider :
        IStoragrRepositoryProvider<IStoragrRepository, IStoragrRepositoryList>
    {
    }

    public interface IStoragrRepositoryProvider<out TItem, out TList>
        where TItem : IStoragrRepository
        where TList : IStoragrRepositoryList
    {
        TItem Repository(string repositoryIdOrName);
        TList Repositories();
    }
}