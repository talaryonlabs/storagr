using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("/")]
    public class RepositoryController : StoragrController
    {
        private readonly IStoreService _storeService;

        public RepositoryController(IStoreService storeService)
        {
            _storeService = storeService;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StoreRepository>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public IEnumerable<StoreRepository> List()
        {
            return _storeService.List();
        }

        [HttpGet("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreRepository))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public StoreRepository Get([FromRoute] string repositoryId)
        {
            if (!_storeService.Exists(repositoryId))
                throw new RepositoryNotFoundError();
            
            return _storeService.Get(repositoryId);
        }
        
        [HttpDelete("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public IActionResult Delete([FromRoute] string repositoryId)
        {
            if (!_storeService.Exists(repositoryId))
                throw new RepositoryNotFoundError();

            _storeService.Delete(repositoryId);

            return Ok();
        }
    }
}