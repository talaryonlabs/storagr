using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;
using Storagr.Shared.Data;
using Storagr.Store.Services;

namespace Storagr.Store.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("{repositoryId}/objects")]
    public class ObjectController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public ObjectController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StoreObject>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public IActionResult List([FromRoute] string repositoryId)
        {
            if (!_storeService.Exists(repositoryId))
                return (ActionResult) new RepositoryNotFoundError();

            return Ok(_storeService.List(repositoryId));
        }

        [HttpGet("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreObject))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public IActionResult Get([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!_storeService.Exists(repositoryId))
                return (ActionResult) new RepositoryNotFoundError();
            if (!_storeService.Exists(repositoryId, objectId))
                return (ActionResult) new ObjectNotFoundError();

            return Ok(_storeService.Get(repositoryId, objectId));
        }
        
        [HttpDelete("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public IActionResult Delete([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!_storeService.Exists(repositoryId))
                return (ActionResult) new RepositoryNotFoundError();
            if (!_storeService.Exists(repositoryId, objectId))
                return (ActionResult) new ObjectNotFoundError();

            _storeService.Delete(repositoryId, objectId);
            
            return Ok();
        }
    }
}