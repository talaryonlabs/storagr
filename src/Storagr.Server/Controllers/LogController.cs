using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Server.Data.Entities;

namespace Storagr.Server.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("logs")]
    [Authorize(Policy = StoragrConstants.ManagementPolicy)]
    public class LogController : StoragrController
    {
        private readonly IDatabaseAdapter _database;

        public LogController(IDatabaseAdapter database)
        {
            _database = database;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLogList))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrLogList> List([FromQuery] StoragrLogListArgs options, CancellationToken cancellationToken)
        {
            const int max = 100;

            var logs = await _database.GetMany<LogEntity>(x =>
            {
                x.OrderBy(o => o.Column("Date", DatabaseOrderType.Desc));
                x.Limit(options.Limit > max ? max : options.Limit);
                x.Offset(options.Skip);
            }, cancellationToken);

            var list = logs.ToList();
            return !list.Any()
                ? new StoragrLogList()
                : new StoragrLogList()
                {
                    Items = list.Select(v => (StoragrLog) v).ToList(),
                    // NextCursor = options.Cursor + list.Count, // TODO
                    TotalCount = await _database.Count<LogEntity>(cancellationToken)
                };
        }
    }
}