using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Services;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    [ApiController]
    [Authorize]
    [Route("{repositoryId}/objects")]
    public class ObjectController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IObjectService _objectService;

        public ObjectController(IUserService userService, IObjectService objectService)
        {
            _userService = userService;
            _objectService = objectService;
        }
        
        
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(StoragrObjectListResponse))]
        [ProducesResponseType(404, Type = typeof(StoragrError))]
        public async Task<IActionResult> List([FromRoute] string repositoryId, [FromQuery] StoragrObjectListRequest request)
        {
            var repository = await _objectService.Get(repositoryId);
            if (repository == null)
                return NotFound(new RepositoryNotFoundError());
            
            var user = await _userService.GetAuthenticatedUser();
            var objects = (await _objectService.GetAll(repositoryId));

            var list = objects.ToList();
            if (!list.Any())
                return Ok(new StoragrObjectListResponse() {Objects = new StoragrObject[0]});

            if (!string.IsNullOrEmpty(request.Cursor))
                list = list.SkipWhile(v => v.ObjectId != request.Cursor).ToList();

            if (request.Limit > 0)
                list = list.Take(request.Limit).ToList();

            return Ok(new StoragrObjectListResponse()
            {
                Objects = list.Select(v => new StoragrObject()
                {
                    ObjectId = v.ObjectId,
                    Size = v.Size,
                }),
                NextCursor = list.Last().ObjectId
            });
        }

        [HttpGet("{objectId}")]
        [ProducesResponseType(200, Type = typeof(StoragrObject))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            var obj = await _objectService.Get(repositoryId, objectId);
            if (obj == null)
                return NotFound();

            return Ok((StoragrObject)obj);
        }
        
        [HttpPost("{objectId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Verify([FromRoute] string repositoryId, [FromRoute] string objectId, [FromBody] StoragrObjectVerifyRequest verifyRequest)
        {
            if (objectId != verifyRequest.ObjectId)
                return BadRequest();

            var repository = await _objectService.Get(repositoryId);
            if (repository == null)
                return NotFound(new RepositoryNotFoundError());

            if (await _objectService.Create(repository.RepositoryId, verifyRequest.ObjectId, verifyRequest.Size) == null)
                return NotFound(new ObjectNotFoundError());

            return Ok();
        }

        [HttpDelete("{objectId}")]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            try
            {
                await _objectService.Delete(repositoryId, objectId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500);
            }
            return NoContent();
        }
    }
}