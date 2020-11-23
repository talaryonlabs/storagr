using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared.Data;
using Storagr.Store.Services;

namespace Storagr.Store.Controllers
{
    [ApiController]
    [Authorize]
    [Route("{repositoryId}/objects")]
    public class ObjectController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public ObjectController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<StoreObject>))]
        [ProducesResponseType(404)]
        public IActionResult List([FromRoute] string repositoryId)
        {
            if (!_storeService.Exists(repositoryId))
                return NotFound();

            return Ok(_storeService.List(repositoryId));
        }

        [HttpGet("{objectId}")]
        [ProducesResponseType(200, Type = typeof(StoreObject))]
        [ProducesResponseType(404)]
        public IActionResult Get([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!_storeService.Exists(repositoryId, objectId))
                return NotFound();

            return Ok(_storeService.Get(repositoryId, objectId));
        }
        
        [HttpDelete("{objectId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult Delete([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!_storeService.Exists(repositoryId, objectId))
                return NotFound();

            _storeService.Delete(repositoryId, objectId);
            
            return Ok();
        }
    }
}