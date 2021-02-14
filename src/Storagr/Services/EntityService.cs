using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Storagr.Services
{
    public class RepositorySearchArgs
    {
        public string OwnerId { get; set; }
    }
    
    public interface IEntityService2
    {
        Task<bool> Exists<TEntity>(string entityId, string parentId = null);
        Task<bool> Exists<TEntity, TSearchArgs>(TSearchArgs searchArgs);
        
        Task<int> Count<TEntity>(string parentId = null);
        Task<int> Count<TEntity, TSearchArgs>(TSearchArgs searchArgs);
        
        // Task<TEntity> Get<TEntity>(string entityId);
        // Task<TEntity> Get<TEntity, TSearchArgs>(TSearchArgs searchArgs);
        
        // Task<IEnumerable<TEntity>> GetMany<TEntity>(IEnumerable<string> entityIds);
        // Task<IEnumerable<TEntity>> GetAll<TEntity>();
        
        // Task Set<TEntity>(TEntity entity);
        // Task SetMany<TEntity>(IEnumerable<TEntity> entityList);
        
        // Task Delete<TEntity>(string entityId);
        // Task DeleteMany<TEntity>(IEnumerable<TEntity> entityList);
    }
    
    // TODO replace Object-, User-, Repository- and LockService with EntityService
    public class EntityService : IEntityService2
    {
        private readonly IBackendAdapter _backend;
        private readonly IDistributedCache _cache;

        public EntityService(IBackendAdapter backend, IDistributedCache cache)
        {
            _backend = backend;
            _cache = cache;
        }

        public async Task<bool> Exists<TEntity>(string entityId, string parentId = null)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Exists<TEntity, TSearchArgs>(TSearchArgs searchArgs)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Count<TEntity>(string parentId = null)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Count<TEntity, TSearchArgs>(TSearchArgs searchArgs)
        {
            throw new NotImplementedException();
        }
    }
}