using System;
using System.Linq;
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
        private readonly IBackendAdapter _backendAdapter;

        public LogController(IBackendAdapter backendAdapter)
        {
            _backendAdapter = backendAdapter;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLogList))]
        public async Task<IActionResult> List([FromQuery] StoragrLogQuery options)
        {
            const int max = 100; 
            
            var logs = await _backendAdapter.GetAll<LogEntity>(x =>
            {
                x.OrderBy(o => o.Column("Date", BackendOrderType.Desc));
                x.Limit(options.Limit > max ? max : options.Limit);
                x.Offset(options.Cursor);
            });

            var list = logs.ToList();
            if (!list.Any())
            {
                return Ok<StoragrLogList>();
            }

            return Ok(new StoragrLogList()
            {
                Items = list.Select(v => (StoragrLog)v).ToList(),
                NextCursor = options.Cursor + list.Count,
                TotalCount = await _backendAdapter.Count<LogEntity>()
            });
        }
    }
}