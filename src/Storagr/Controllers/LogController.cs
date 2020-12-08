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
    public class LogController : ControllerBase
    {
        private readonly IBackendAdapter _backendAdapter;

        public LogController(IBackendAdapter backendAdapter)
        {
            _backendAdapter = backendAdapter;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLogList))]
        public async Task<IActionResult> List([FromQuery] StoragrLogListOptions options)
        {
            const int max = 100; 
            
            var logs = await _backendAdapter.GetAll<LogEntity>(x =>
            {
                x.OrderBy(o => o.Column("Date", BackendOrderType.Desc));
                x.Limit(options.Limit > max ? max : options.Limit);
                x.Offset(options.Offset);
            });

            var list = logs.ToList();
            if (!list.Any())
            {
                return Ok(StoragrLogList.Empty);
            }

            return Ok(new StoragrLogList()
            {
                Logs = list.Select(v => (StoragrLog)v).ToList(),
                Cursor = options.Offset + list.Count,
                Total = await _backendAdapter.Count<LogEntity>()
            });
        }
    }
}