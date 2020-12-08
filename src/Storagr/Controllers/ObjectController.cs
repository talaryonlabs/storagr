using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Services;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("{repositoryId}/objects")]
    [Authorize(Policy = StoragrConstants.ManagementPolicy)]
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrObjectList))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> List([FromRoute] string repositoryId, [FromQuery] StoragrObjectListOptions options)
        {
            var repository = await _objectService.Get(repositoryId);
            if (repository == null)
                return (ActionResult) new RepositoryNotFoundError();
            
            var user = await _userService.GetAuthenticatedUser();
            var objects = (await _objectService.GetAll(repositoryId));

            var list = objects.ToList();
            if (!list.Any())
                return Ok(StoragrObjectList.Empty);

            if (!string.IsNullOrEmpty(options.Cursor))
                list = list.SkipWhile(v => v.Id != options.Cursor).ToList();

            if (options.Limit > 0)
                list = list.Take(options.Limit).ToList();

            return Ok(new StoragrObjectList()
            {
                Objects = list.Select(v => new StoragrObject()
                {
                    ObjectId = v.Id,
                    Size = v.Size,
                }),
                NextCursor = list.Last().Id
            });
        }

        [HttpGet("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrObject))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Get([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            var obj = await _objectService.Get(repositoryId, objectId);
            if (obj == null)
                return (ActionResult) new ObjectNotFoundError();

            return Ok((StoragrObject)obj);
        }
        
        [HttpPost("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Verify([FromRoute] string repositoryId, [FromRoute] string objectId, [FromBody] StoragrObject expectedObject)
        {
            if (objectId != expectedObject.ObjectId)
                return BadRequest();

            var repository = await _objectService.Get(repositoryId);
            if (repository == null)
                return (ActionResult) new RepositoryNotFoundError();

            if (await _objectService.Create(repository.Id, expectedObject.ObjectId, expectedObject.Size) == null)
                return (ActionResult) new ObjectNotFoundError();

            return Ok();
        }

        [HttpDelete("{objectId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Delete([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            var repository = await _objectService.Get(repositoryId);
            if (repository == null)
                return (ActionResult) new RepositoryNotFoundError();
            
            var obj = await _objectService.Get(repositoryId, objectId);
            if (obj == null)
                return (ActionResult) new ObjectNotFoundError();
            
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