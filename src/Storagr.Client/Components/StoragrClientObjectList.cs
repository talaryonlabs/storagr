using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Data;

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