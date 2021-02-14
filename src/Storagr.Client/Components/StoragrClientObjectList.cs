using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientObjectList : 
        IStoragrClientObjectList, 
        IStoragrObjectParams
    {
        private readonly IStoragrClientRequest _clientRequest;
        private readonly string _repositoryIdOrName;
        private readonly StoragrObjectListArgs _listArgs;

        public StoragrClientObjectList(IStoragrClientRequest clientRequest, string repositoryIdOrName) 
        {
            _clientRequest = clientRequest;
            _repositoryIdOrName = repositoryIdOrName;
            _listArgs = new StoragrObjectListArgs();
        }
        
        IStoragrList<StoragrObject> IStoragrRunner<IStoragrList<StoragrObject>>.Run() => (this as IStoragrRunner<IStoragrList<StoragrObject>>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        async Task<IStoragrList<StoragrObject>> IStoragrRunner<IStoragrList<StoragrObject>>.RunAsync(CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return await _clientRequest.Send<StoragrObjectList>(
                $"repositories/{_repositoryIdOrName}/objects?{query}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        IStoragrListable<StoragrObject, IStoragrObjectParams> IStoragrListable<StoragrObject, IStoragrObjectParams>.Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        IStoragrListable<StoragrObject, IStoragrObjectParams> IStoragrListable<StoragrObject, IStoragrObjectParams>.Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        IStoragrListable<StoragrObject, IStoragrObjectParams> IStoragrListable<StoragrObject, IStoragrObjectParams>.SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        IStoragrListable<StoragrObject, IStoragrObjectParams> IStoragrListable<StoragrObject, IStoragrObjectParams>.Where(Action<IStoragrObjectParams> whereParams)
        {
            whereParams(this);
            return this;
        }

        IStoragrObjectParams IStoragrObjectParams.Id(string objectId)
        {
            _listArgs.Id = objectId;
            return this;
        }
    }
}