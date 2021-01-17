using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared;
using Storagr.Shared.Data;

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

        protected override Task<IStoragrList<StoragrRepository>> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return clientRequest.Send<IStoragrList<StoragrRepository>>(
                $"repositories?{query}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        IStoragrClientList<StoragrRepository, IStoragrRepositoryParams> IStoragrClientList<StoragrRepository, IStoragrRepositoryParams>.Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        IStoragrClientList<StoragrRepository, IStoragrRepositoryParams> IStoragrClientList<StoragrRepository, IStoragrRepositoryParams>.Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        IStoragrClientList<StoragrRepository, IStoragrRepositoryParams> IStoragrClientList<StoragrRepository, IStoragrRepositoryParams>.SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        IStoragrClientList<StoragrRepository, IStoragrRepositoryParams> IStoragrClientList<StoragrRepository, IStoragrRepositoryParams>.Where(Action<IStoragrRepositoryParams> whereParams)
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