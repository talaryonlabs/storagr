using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientLogList : 
        IStoragrClientLogList, 
        IStoragrLogParams
    {
        private readonly IStoragrClientRequest _clientRequest;
        private readonly StoragrLogListArgs _listArgs;
        
        public StoragrClientLogList(IStoragrClientRequest clientRequest)
        {
            _clientRequest = clientRequest;
            _listArgs = new StoragrLogListArgs();
        }
        
        IStoragrList<StoragrLog> IStoragrRunner<IStoragrList<StoragrLog>>.Run() => (this as IStoragrRunner<IStoragrList<StoragrLog>>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        async Task<IStoragrList<StoragrLog>> IStoragrRunner<IStoragrList<StoragrLog>>.RunAsync(CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return await _clientRequest.Send<StoragrLogList>(
                $"logs?{query}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        IStoragrListable<StoragrLog, IStoragrLogParams> IStoragrListable<StoragrLog, IStoragrLogParams>.Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        IStoragrListable<StoragrLog, IStoragrLogParams> IStoragrListable<StoragrLog, IStoragrLogParams>.Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        IStoragrListable<StoragrLog, IStoragrLogParams> IStoragrListable<StoragrLog, IStoragrLogParams>.SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        IStoragrListable<StoragrLog, IStoragrLogParams> IStoragrListable<StoragrLog, IStoragrLogParams>.Where(Action<IStoragrLogParams> whereParams)
        {
            whereParams(this);
            return this;
        }

        IStoragrLogParams IStoragrLogParams.Message(string message)
        {
            _listArgs.Message = message;
            return this;
        }

        IStoragrLogParams IStoragrLogParams.Level(LogLevel logLevel)
        {
            _listArgs.Level = logLevel;
            return this;
        }

        IStoragrLogParams IStoragrLogParams.Category(string category)
        {
            _listArgs.Category = category;
            return this;
        }
    }
}