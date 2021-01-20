using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientObjectList : 
        StoragrClientHelper<IStoragrList<StoragrObject>>, 
        IStoragrClientObjectList, 
        IStoragrObjectParams
    {
        private readonly string _repositoryIdOrName;
        private readonly StoragrObjectListArgs _listArgs;

        public StoragrClientObjectList(IStoragrClientRequest clientRequest, string repositoryIdOrName) 
            : base(clientRequest)
        {
            _repositoryIdOrName = repositoryIdOrName;
            _listArgs = new StoragrObjectListArgs();
        }

        protected override async Task<IStoragrList<StoragrObject>> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return await clientRequest.Send<StoragrObjectList>(
                $"repositories/{_repositoryIdOrName}/objects?{query}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        IStoragrClientList<StoragrObject, IStoragrObjectParams> IStoragrClientList<StoragrObject, IStoragrObjectParams>.Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        IStoragrClientList<StoragrObject, IStoragrObjectParams> IStoragrClientList<StoragrObject, IStoragrObjectParams>.Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        IStoragrClientList<StoragrObject, IStoragrObjectParams> IStoragrClientList<StoragrObject, IStoragrObjectParams>.SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        IStoragrClientList<StoragrObject, IStoragrObjectParams> IStoragrClientList<StoragrObject, IStoragrObjectParams>.Where(Action<IStoragrObjectParams> whereParams)
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