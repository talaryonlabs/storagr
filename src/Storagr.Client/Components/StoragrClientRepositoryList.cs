using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientRepositoryList : StoragrClientHelper, IStoragrClientRepositoryList, IStoragrRepositoryParams
    {
        private readonly StoragrRepositoryListArgs _listArgs;

        public StoragrClientRepositoryList(IStoragrClientRequest clientRequest) 
            : base(clientRequest)
        {
            _listArgs = new StoragrRepositoryListArgs();
        }

        IStoragrList<StoragrRepository> IStoragrClientRunner<IStoragrList<StoragrRepository>>.Run()
        {
            var task = (this as IStoragrClientRepositoryList).RunAsync();
            task.RunSynchronously();
            return task.Result;
        }

        Task<IStoragrList<StoragrRepository>> IStoragrClientRunner<IStoragrList<StoragrRepository>>.RunAsync(CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return Request<IStoragrList<StoragrRepository>>($"repositories?{query}", HttpMethod.Get, cancellationToken);
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