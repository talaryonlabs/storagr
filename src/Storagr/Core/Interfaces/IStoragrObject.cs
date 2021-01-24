using Storagr.Data;

namespace Storagr
{
    public interface IStoragrObject :
        IStoragrExistable,
        IStoragrRunner<StoragrObject>,
        IStoragrDeletable<StoragrObject>
    {
        
    }

    public interface IStoragrObjectList :
        IStoragrCountable,
        IStoragrEnumerable<StoragrObject, IStoragrObjectParams>
    {
        
    }


    
    public interface IStoragrObjectProvider : 
        IStoragrObjectProvider<IStoragrObject>
    {
        
    }
    
    public interface IStoragrObjectProvider<out TItem> : 
        IStoragrObjectProvider<TItem, IStoragrObjectList> 
        where TItem : IStoragrObject
    {
        
    }
    
    public interface IStoragrObjectProvider<out TItem, out TList>
        where TItem : IStoragrObject
        where TList : IStoragrObjectList
    {
        TItem Object(string objectId);
        TList Objects();
    }
}