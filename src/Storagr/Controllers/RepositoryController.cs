using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/")]
    public class RepositoryController : ControllerBase
    {
        private readonly IObjectService _objectService;

        public RepositoryController(IObjectService objectService)
        {
            _objectService = objectService;
        }
        
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<StoragrRepository>))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> List()
        {
            return Ok((await _objectService.GetAll()).Select(v => (StoragrRepository) v));
        }
        
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(StoragrRepository))]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Create([FromBody] StoragrRepository newRepository)
        {
            var repository = await _objectService.Get(newRepository.RepositoryId);
            if (repository == null)
                return Conflict(new RepositoryAlreadyExistsError(newRepository));
            
            // TODO create repository

            return Created($"/{repository.RepositoryId}", (StoragrRepository)repository);
        }

        [HttpGet("{repositoryId}")]
        [ProducesResponseType(200, Type = typeof(StoragrRepository))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get([FromRoute] string repositoryId)
        {
            var repository = await _objectService.Get(repositoryId);
            if (repository == null)
                return NotFound();

            return Ok((StoragrRepository)repository);
        }
        
        [HttpDelete("{repositoryId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete([FromRoute] string repositoryId)
        {
            var repository = await _objectService.Get(repositoryId);
            if (repository == null)
                return NotFound();

            await _objectService.Delete(repositoryId);
            
            return Ok();
        }
    }
}