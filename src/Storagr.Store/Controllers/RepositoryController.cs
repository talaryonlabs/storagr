using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/")]
    public class RepositoryController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public RepositoryController(IStoreService storeService)
        {
            _storeService = storeService;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StoreRepository>))]
        public IActionResult List()
        {
            return Ok(_storeService.List());
        }

        [HttpGet("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreRepository))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public IActionResult Get([FromRoute] string repositoryId)
        {
            if (!_storeService.Exists(repositoryId))
                return (ActionResult) new RepositoryNotFoundError();

            return Ok(_storeService.Get(repositoryId));
        }
        
        [HttpDelete("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public IActionResult Delete([FromRoute] string repositoryId)
        {
            if (!_storeService.Exists(repositoryId))
                return (ActionResult) new RepositoryNotFoundError();

            _storeService.Delete(repositoryId);
            
            return Ok();
        }
    }
}