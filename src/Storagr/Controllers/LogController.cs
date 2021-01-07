using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("logs")]
    [Authorize(Policy = StoragrConstants.ManagementPolicy)]
    public class LogController : StoragrController
    {
        private readonly IDatabaseAdapter _backendAdapter;

        public LogController(IDatabaseAdapter backendAdapter)
        {
            _backendAdapter = backendAdapter;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLogList))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrLogList> List([FromQuery] StoragrLogQuery options, CancellationToken cancellationToken)
        {
            const int max = 100;

            var logs = await _backendAdapter.GetMany<LogEntity>(x =>
            {
                x.OrderBy(o => o.Column("Date", DatabaseOrderType.Desc));
                x.Limit(options.Limit > max ? max : options.Limit);
                x.Offset(options.Cursor);
            }, cancellationToken);

            var list = logs.ToList();
            return !list.Any()
                ? new StoragrLogList()
                : new StoragrLogList()
                {
                    Items = list.Select(v => (StoragrLog) v).ToList(),
                    NextCursor = options.Cursor + list.Count,
                    TotalCount = await _backendAdapter.Count<LogEntity>(cancellationToken)
                };
        }
    }
}