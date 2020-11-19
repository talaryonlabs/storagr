using System.Collections.Generic;
using System.Threading.Tasks;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.IO;

namespace Storagr.Services
{
    public interface IObjectService
    {
        Task<ObjectEntity> Get(string repositoryId, string objectId);
        Task<IEnumerable<ObjectEntity>> GetMany(string repositoryId, params string[] objectIds);
        Task<IEnumerable<ObjectEntity>> GetAll(string repositoryId);

        Task Delete(string repositoryId, string objectId);
        Task DeleteAll(string repositoryId);
    }
    
    public class ObjectService : IObjectService
    {
        private readonly IBackendAdapter _backend;
        private readonly IStoreAdapter _store;

        public ObjectService(IBackendAdapter backend, IStoreAdapter store)
        {
            _backend = backend;
            _store = store;
        }

        public async Task<ObjectEntity> Get(string repositoryId, string objectId)
        {
            return await _backend.Get<ObjectEntity>(q =>
            {
                q.Where(f =>
                {
                    f.Equal("RepositoryId", repositoryId)
                        .And()
                        .Equal("ObjectId", objectId);
                });
            });
        }

        public async Task<IEnumerable<ObjectEntity>> GetMany(string repositoryId, params string[] objectIds)
        {
            return await _backend.GetAll<ObjectEntity>(q =>
            {
                q.Where(f =>
                {
                    f.Equal("RepositoryId", repositoryId)
                        .And()
                        .In("ObjectId", objectIds);
                });
            });
        }

        public async Task<IEnumerable<ObjectEntity>> GetAll(string repositoryId)
        {
            return await _backend.GetAll<ObjectEntity>(q =>
            {
                q.Where(f =>
                {
                    f.Equal("RepositoryId", repositoryId);
                });
            });
        }

        public async Task Delete(string repositoryId, string objectId)
        {
            var obj = await Get(repositoryId, objectId);
            
            await _backend.Delete(obj);
        }

        public async Task DeleteAll(string repositoryId)
        {
            var list = await GetAll(repositoryId);

            await _backend.Delete(list);
        }
    }
}