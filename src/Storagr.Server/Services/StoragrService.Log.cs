using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Storagr.Server.Data.Entities;
using Storagr.Shared;

namespace Storagr.Server.Services
{
    public interface IStoragrLogList :
        IStoragrEnumerable<LogEntity, ILogParams>,
        IStoragrCountable
    {
        
    }
    
    public partial class StoragrService
    {
        private class LogList :
            IStoragrLogList,
            ILogParams
        {
            private readonly StoragrService _storagrService;
            private readonly LogEntity _entity;

            private int _take, _skip;
            private string _skipUntil;

            public LogList(StoragrService storagrService)
            {
                _storagrService = storagrService;
                _entity = new LogEntity();
            }
        
            IEnumerable<LogEntity> IStoragrRunner<IEnumerable<LogEntity>>.Run() =>
                (this as IStoragrRunner<IEnumerable<LogEntity>>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<IEnumerable<LogEntity>> IStoragrRunner<IEnumerable<LogEntity>>.RunAsync(CancellationToken cancellationToken)
            {
                var logs = await _storagrService
                    .Database
                    .Many<LogEntity>()
                    .Where(filter =>
                    {
                        if (_entity.Category is not null)
                        {
                            filter
                                .Is(nameof(LogEntity.Category))
                                .Like(_entity.Category)
                                .Or();
                        }

                        if (_entity.Message is not null)
                        {
                            filter
                                .Is(nameof(LogEntity.Message))
                                .Like(_entity.Message)
                                .Or();
                        }
                    })
                    .OrderBy(order => order.Desc(nameof(LogEntity.Date)))
                    .RunAsync(cancellationToken);

                return logs
                    .Skip(_skip)
                    .Take(_take);
            }
            
            IStoragrRunner<int> IStoragrCountable.Count()
            {
                return _storagrService
                    .Database
                    .Count<LogEntity>();
            }

            IStoragrEnumerable<LogEntity, ILogParams> IStoragrEnumerable<LogEntity, ILogParams>.Take(int count)
            {
                _take = count;
                return this;
            }

            IStoragrEnumerable<LogEntity, ILogParams> IStoragrEnumerable<LogEntity, ILogParams>.Skip(int count)
            {
                _skip = count;
                return this;
            }

            IStoragrEnumerable<LogEntity, ILogParams> IStoragrEnumerable<LogEntity, ILogParams>.SkipUntil(string cursor)
            {
                _skipUntil = cursor;
                return this;
            }

            IStoragrEnumerable<LogEntity, ILogParams> IStoragrEnumerable<LogEntity, ILogParams>.Where(Action<ILogParams> whereParams)
            {
                whereParams(this);
                return this;
            }

            ILogParams ILogParams.Message(string message)
            {
                _entity.Message = message;
                return this;
            }

            ILogParams ILogParams.Level(LogLevel level)
            {
                _entity.Level = level;
                return this;
            }

            ILogParams ILogParams.Category(string category)
            {
                _entity.Category = category;
                return this;
            }
        }
    }
}