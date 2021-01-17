using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientLogList : 
        StoragrClientHelper<IStoragrList<StoragrLog>>, 
        IStoragrLogList, 
        IStoragrLogParams
    {
        private readonly StoragrLogListArgs _listArgs;
        
        public StoragrClientLogList(IStoragrClientRequest clientRequest) 
            : base(clientRequest)
        {
            _listArgs = new StoragrLogListArgs();
        }

        protected override Task<IStoragrList<StoragrLog>> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return clientRequest.Send<IStoragrList<StoragrLog>>(
                $"logs?{query}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        IStoragrClientList<StoragrLog, IStoragrLogParams> IStoragrClientList<StoragrLog, IStoragrLogParams>.Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        IStoragrClientList<StoragrLog, IStoragrLogParams> IStoragrClientList<StoragrLog, IStoragrLogParams>.Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        IStoragrClientList<StoragrLog, IStoragrLogParams> IStoragrClientList<StoragrLog, IStoragrLogParams>.SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        IStoragrClientList<StoragrLog, IStoragrLogParams> IStoragrClientList<StoragrLog, IStoragrLogParams>.Where(Action<IStoragrLogParams> whereParams)
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