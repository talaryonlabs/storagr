using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Server.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("logs")]
    [Authorize(Policy = StoragrConstants.ManagementPolicy)]
    public class LogController : StoragrController
    {
        private readonly IStoragrService _storagrService;

        public LogController(IStoragrService storagrService)
        {
            _storagrService = storagrService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLogList))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrLogList> List([FromQuery] StoragrLogListArgs listArgs, CancellationToken cancellationToken)
        {
            var count = await _storagrService
                .Repositories()
                .Count()
                .RunAsync(cancellationToken);
            
            if (count == 0)
                return new StoragrLogList();

            var list = (
                await _storagrService
                    .Logs()
                    .Skip(listArgs.Skip)
                    .SkipUntil(listArgs.Cursor)
                    .Take(listArgs.Limit)
                    .Where(whereParams => whereParams
                        .Category(listArgs.Category)
                        .Level(listArgs.Level)
                        .Message(listArgs.Message)
                    )
                    .RunAsync(cancellationToken)
            ).ToList();

            return !list.Any()
                ? new StoragrLogList()
                : new StoragrLogList()
                {
                    Items = list.Select(v => (StoragrLog) v),
                    NextCursor = null,
                    TotalCount = count
                };
        }
    }
}