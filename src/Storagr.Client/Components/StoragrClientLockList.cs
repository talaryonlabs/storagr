using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientLockList : StoragrClientHelper, IStoragrClientLockList, IStoragrLockParams
    {
        private readonly string _repositoryId;
        private readonly StoragrLockListArgs _listArgs;

        public StoragrClientLockList(IStoragrClientRequest clientRequest, string repositoryId) 
            : base(clientRequest)
        {
            _repositoryId = repositoryId;
            _listArgs = new StoragrLockListArgs();
        }

        IStoragrList<StoragrLock> IStoragrClientRunner<IStoragrList<StoragrLock>>.Run()
        {
            var task = (this as IStoragrClientLockList).RunAsync();
            task.RunSynchronously();
            return task.Result;
        }

        Task<IStoragrList<StoragrLock>> IStoragrClientRunner<IStoragrList<StoragrLock>>.RunAsync(CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return Request<IStoragrList<StoragrLock>>($"repositories/{_repositoryId}/locks?{query}", HttpMethod.Get, cancellationToken);
        }

        IStoragrClientList<StoragrLock, IStoragrLockParams> IStoragrClientList<StoragrLock, IStoragrLockParams>.Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        IStoragrClientList<StoragrLock, IStoragrLockParams> IStoragrClientList<StoragrLock, IStoragrLockParams>.Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        IStoragrClientList<StoragrLock, IStoragrLockParams> IStoragrClientList<StoragrLock, IStoragrLockParams>.SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        IStoragrClientList<StoragrLock, IStoragrLockParams> IStoragrClientList<StoragrLock, IStoragrLockParams>.Where(Action<IStoragrLockParams> whereParams)
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
    }
}