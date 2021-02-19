using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Server.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server.Services
{
    public class BatchService :
        IBatchService,
        IBatchServiceRepository,
        IBatchServiceObjects,
        IBatchServiceOperation,
        IBatchParams
    {
        private readonly IDatabaseAdapter _database;
        private readonly IStoreAdapter _store;
        private readonly IStoragrService _storagrService;

        private IEnumerable<StoragrObject> _objects;
        private bool _download;
        private bool _upload;
        private IEnumerable<string> _transfers;
        private string _ref;
        private string _repositoryIdOrName;

        public BatchService(IDatabaseAdapter database, IStoreAdapter store, IStoragrService storagrService)
        {
            _database = database;
            _store = store;
            _storagrService = storagrService;
        }

        IEnumerable<StoragrBatchObject> IStoragrRunner<IEnumerable<StoragrBatchObject>>.Run() =>
            (this as IStoragrRunner<IEnumerable<StoragrBatchObject>>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        async Task<IEnumerable<StoragrBatchObject>> IStoragrRunner<IEnumerable<StoragrBatchObject>>.RunAsync(CancellationToken cancellationToken)
        {
            var repository = await _storagrService
                .Repository(_repositoryIdOrName)
                .RunAsync(cancellationToken);

            var authenticatedUser = await _storagrService
                .Authorization()
                .GetAuthenticatedUser()
                .RunAsync(cancellationToken);

            var hasAccess = await _storagrService
                .Repository(_repositoryIdOrName)
                .HasAccess(authenticatedUser.Id, _download ? RepositoryAccessType.Write : RepositoryAccessType.Read)
                .RunAsync(cancellationToken);

            if (!hasAccess)
                throw new ForbiddenError();

            var objects = await _database
                .Many<ObjectEntity>()
                .Where(filter => filter
                    .Is(nameof(ObjectEntity.RepositoryId))
                    .EqualTo(repository.Id)
                    .And()
                    .Is(nameof(ObjectEntity.Id))
                    .In(_objects.Select(o => o.ObjectId))
                )
                .RunAsync(cancellationToken);
            
            return _objects.Select(storagrObject =>
            {
                var batchObject = new StoragrBatchObject()
                {
                    ObjectId = storagrObject.ObjectId,
                    Size = storagrObject.Size,
                    Authenticated = true
                };
                var e = objects.FirstOrDefault(o => o.Id == storagrObject.ObjectId && o.Size == storagrObject.Size);

                if (_upload && e is not null) // already uploaded, ignore actions request
                    return batchObject;

                
                
                if (_download)
                {
                    if (e is null)
                        batchObject.Error = new ObjectNotFoundError();
                    else
                    {
                        batchObject.Actions = new StoragrActions()
                        {
                            Download = _store
                                .Object(storagrObject.ObjectId)
                                .Download()
                                .Run()
                        };
                    }
                }
                else if(_upload)
                {
                    batchObject.Actions = new StoragrActions()
                    {
                        Upload = _store
                            .Object(storagrObject.ObjectId)
                            .Upload()
                            .Run(),
                        Verify = null // TODO
                    };
                }
                
                return batchObject;
            });
        }
        
        public IBatchServiceRepository Repository(string repositoryIdOrName)
        {
            _repositoryIdOrName = repositoryIdOrName;
            return this;
        }

        IBatchServiceObjects IBatchServiceRepository.Objects(IEnumerable<StoragrObject> objects)
        {
            _objects = objects;
            return this;
        }

        IBatchServiceOperation IBatchServiceObjects.Download()
        {
            _download = true;
            return this;
        }

        IBatchServiceOperation IBatchServiceObjects.Upload()
        {
            _upload = true;
            return this;
        }

        IStoragrRunner<IEnumerable<StoragrBatchObject>> IStoragrParams<IEnumerable<StoragrBatchObject>, IBatchParams>.With(
            Action<IBatchParams> withParams)
        {
            withParams(this);
            return this;
        }

        IBatchParams IBatchParams.Transfers(IEnumerable<string> transfers)
        {
            _transfers = transfers;
            return this;
        }

        IBatchParams IBatchParams.Ref(string name)
        {
            _ref = name;
            return this;
        }
    }
}