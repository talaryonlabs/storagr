using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    [ApiController]
    [Authorize(Policy = StoragrConstants.ManagementPolicy)]
    [Route("/")]
    public class RepositoryController : ControllerBase
    {
        private readonly IObjectService _objectService;

        public RepositoryController(IObjectService objectService)
        {
            _objectService = objectService;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StoragrRepository>))]
        public async Task<IActionResult> List()
        {
            return Ok((await _objectService.GetAll()).Select(v => (StoragrRepository) v));
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(StoragrRepository))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(StoragrError))]
        public async Task<IActionResult> Create([FromBody] StoragrRepository newRepository)
        {
            var repository = await _objectService.Get(newRepository.RepositoryId);
            if (repository == null)
                return (ActionResult) new RepositoryAlreadyExistsError(newRepository);

            if ((repository = await _objectService.Create(newRepository.RepositoryId, newRepository.OwnerId)) == null)
                return (ActionResult) new StoragrError("Unable to create repository.");

            return Created($"/{repository.Id}", (StoragrRepository)repository);
        }

        [HttpGet("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrRepository))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Get([FromRoute] string repositoryId)
        {
            var repository = await _objectService.Get(repositoryId);
            if (repository == null)
                return (ActionResult) new RepositoryNotFoundError();

            return Ok((StoragrRepository)repository);
        }
        
        [HttpDelete("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Delete([FromRoute] string repositoryId)
        {
            var repository = await _objectService.Get(repositoryId);
            if (repository == null)
                return (ActionResult) new RepositoryNotFoundError();

            await _objectService.Delete(repositoryId);
            
            return NoContent();
        }
    }
}