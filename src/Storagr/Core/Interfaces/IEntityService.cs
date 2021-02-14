using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storagr
{
    public interface IEntityService<TEntity>
        where TEntity : class
    {
        Task<bool> Exists(string entityId);
        Task<int> Count();

        Task<TEntity> Get(string entityId);
        Task<IEnumerable<TEntity>> GetMany(IEnumerable<string> entityIds);
        Task<IEnumerable<TEntity>> GetAll();
        
        Task Create(TEntity entity);
        Task Delete(string entityId);
    }
    
    public interface IEntityService<TEntity, in TSearchArgs> : IEntityService<TEntity>
        where TEntity : class
        where TSearchArgs : struct
    {
        Task<IEnumerable<TEntity>> GetAll(TSearchArgs searchArgs);
    }

    public interface IContainerService<TEntity>
        where TEntity : class
    {
        Task<bool> Exists(string containerId, string objectId);
        Task<int> Count(string containerId);

        Task<TEntity> Get(string containerId, string entityId);
        Task<IEnumerable<TEntity>> GetMany(string containerId, IEnumerable<string> entityIds);
        Task<IEnumerable<TEntity>> GetAll(string containerId);
        
        Task Create(string containerId, TEntity entity);
        Task Delete(string containerId, string entityId);
        Task DeleteAll(string containerId);
    }
    
    public interface IContainerService<TEntity, in TSearchArgs> : IContainerService<TEntity>
        where TEntity : class
        where TSearchArgs : struct
    {
        Task<IEnumerable<TEntity>> GetAll(string containerId, TSearchArgs searchArgs);
    }
}