using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared.Data;
using Storagr.Store.Services;

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
        [ProducesResponseType(200, Type = typeof(IEnumerable<StoreRepository>))]
        [ProducesResponseType(404)]
        public IActionResult List()
        {
            return Ok(_storeService.List());
        }
        
        [HttpGet("{repositoryId}")]
        [ProducesResponseType(200, Type = typeof(StoreRepository))]
        [ProducesResponseType(404)]
        public IActionResult Get([FromRoute] string repositoryId)
        {
            if (!_storeService.Exists(repositoryId))
                return NotFound();

            return Ok(_storeService.Get(repositoryId));
        }
        
        [HttpDelete("{repositoryId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult Delete([FromRoute] string repositoryId)
        {
            if (!_storeService.Exists(repositoryId))
                return NotFound();

            _storeService.Delete(repositoryId);
            
            return Ok();
        }
    }
}