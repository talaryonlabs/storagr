using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Client.Params;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientLockList : 
        IStoragrClientLockList, 
        IStoragrLockParams
    {
        private readonly IStoragrClientRequest _clientRequest;
        private readonly string _repositoryId;
        private readonly StoragrLockListArgs _listArgs;

        public StoragrClientLockList(IStoragrClientRequest clientRequest, string repositoryId) 
        {
            _clientRequest = clientRequest;
            _repositoryId = repositoryId;
            _listArgs = new StoragrLockListArgs();
        }
        
        IStoragrList<StoragrLock> IStoragrRunner<IStoragrList<StoragrLock>>.Run() => (this as IStoragrRunner<IStoragrList<StoragrLock>>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        async Task<IStoragrList<StoragrLock>> IStoragrRunner<IStoragrList<StoragrLock>>.RunAsync(CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return await _clientRequest.Send<StoragrLockList>(
                $"repositories/{_repositoryId}/locks?{query}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        IStoragrListable<StoragrLock, IStoragrLockParams> IStoragrListable<StoragrLock, IStoragrLockParams>.Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        IStoragrListable<StoragrLock, IStoragrLockParams> IStoragrListable<StoragrLock, IStoragrLockParams>.Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        IStoragrListable<StoragrLock, IStoragrLockParams> IStoragrListable<StoragrLock, IStoragrLockParams>.SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        IStoragrListable<StoragrLock, IStoragrLockParams> IStoragrListable<StoragrLock, IStoragrLockParams>.Where(Action<IStoragrLockParams> whereParams)
        {
            whereParams(this);
            return this;
        }

        IStoragrLockParams IStoragrLockParams.Id(string lockId)
        {
            _listArgs.Id = lockId;
            return this;
        }

        IStoragrLockParams IStoragrLockParams.Path(string lockedPath)
        {
            _listArgs.Path = lockedPath;
            return this;
        }

        IStoragrLockParams IStoragrLockParams.Owner(string owner)
        {
            _listArgs.Owner = owner;
            return this;
        }
    }
}