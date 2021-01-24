using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Storagr;
using Storagr.Data;

namespace Storagr.Client
{
    internal class StoragrClientClientLogList : 
        StoragrClientHelper<IStoragrList<StoragrLog>>, 
        IStoragrClientLogList, 
        IStoragrLogParams
    {
        private readonly StoragrLogListArgs _listArgs;
        
        public StoragrClientClientLogList(IStoragrClientRequest clientRequest) 
            : base(clientRequest)
        {
            _listArgs = new StoragrLogListArgs();
        }

        protected override async Task<IStoragrList<StoragrLog>> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return await clientRequest.Send<StoragrLogList>(
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