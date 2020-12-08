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
    public class ObjectController : StoragrController
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
            return !_storeService.Exists(repositoryId) ? Error<RepositoryNotFoundError>() : Ok(_storeService.List(repositoryId));
        }

        [HttpGet("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreObject))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public IActionResult Get([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!_storeService.Exists(repositoryId))
                return Error<RepositoryNotFoundError>();
            if (!_storeService.Exists(repositoryId, objectId))
                return Error<ObjectNotFoundError>();

            var obj = _storeService.Get(repositoryId, objectId);
            return Ok(obj);
        }
        
        [HttpDelete("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public IActionResult Delete([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!_storeService.Exists(repositoryId))
                return Error<RepositoryNotFoundError>();
            if (!_storeService.Exists(repositoryId, objectId))
                return Error<ObjectNotFoundError>();

            _storeService.Delete(repositoryId, objectId);
            
            return Ok();
        }
    }
}