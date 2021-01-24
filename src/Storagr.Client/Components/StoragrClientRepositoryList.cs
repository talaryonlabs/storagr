using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Data;

namespace Storagr.Client
{
    internal class StoragrClientRepositoryList : 
        StoragrClientHelper<IStoragrList<StoragrRepository>>, 
        IStoragrClientRepositoryList, 
        IStoragrRepositoryParams
    {
        private readonly StoragrRepositoryListArgs _listArgs;

        public StoragrClientRepositoryList(IStoragrClientRequest clientRequest) 
            : base(clientRequest)
        {
            _listArgs = new StoragrRepositoryListArgs();
        }

        protected override async Task<IStoragrList<StoragrRepository>> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return await clientRequest.Send<StoragrRepositoryList>(
                $"repositories?{query}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        IStoragrListable<StoragrRepository, IStoragrRepositoryParams> IStoragrListable<StoragrRepository, IStoragrRepositoryParams>.Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        IStoragrListable<StoragrRepository, IStoragrRepositoryParams> IStoragrListable<StoragrRepository, IStoragrRepositoryParams>.Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        IStoragrListable<StoragrRepository, IStoragrRepositoryParams> IStoragrListable<StoragrRepository, IStoragrRepositoryParams>.SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        IStoragrListable<StoragrRepository, IStoragrRepositoryParams> IStoragrListable<StoragrRepository, IStoragrRepositoryParams>.Where(Action<IStoragrRepositoryParams> whereParams)
        {
            whereParams(this);
            return this;
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.Id(string repositoryId)
        {
            _listArgs.Id = repositoryId;
            return this;
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.Name(string name)
        {
            _listArgs.Name = name;
            return this;
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.Owner(string owner)
        {
            _listArgs.Owner = owner;
            return this;
        }

        IStoragrRepositoryParams IStoragrRepositoryParams.SizeLimit(ulong sizeLimit)
        {
            _listArgs.SizeLimit = sizeLimit;
            return this;
        }
    }
}