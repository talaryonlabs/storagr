using System.Collections.Generic;

namespace Storagr.Store
{
    public interface IStoreService
    {
        int BufferSize { get; }
        
        IStoreRepository Repository(string repositoryId);
        IEnumerable<IStoreRepository> Repositories();
    }

    public interface IStoreExistable
    {
        bool Exists();
    }
    
    public interface IStoreDeletable
    {
        void Delete();
    }

    public interface IStoreModel<out T>
    {
        T Model();
    }

    public interface IStoreMeta
    {
        string Id { get; }
        ulong Size { get; }
    }
    
    public interface IStoreMetaName
    {
        string Name { get; }
    }


}