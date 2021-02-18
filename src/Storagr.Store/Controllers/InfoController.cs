using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("info")]
    public class InfoController : StoragrController
    {
        private readonly IStoreService _storeService;
        private readonly ILogger<InfoController> _logger;

        public InfoController(IStoreService storeService, ILogger<InfoController> logger)
        {
            _storeService = storeService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StoreObject>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public StoreInformation Info() => new()
        {
            AvailableSpace = _storeService.AvailableSpace,
            UsedSpace = _storeService.UsedSpace
        };
    }
}