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
    public class ObjectController : StoragrController
    {
        private readonly IObjectService _objectService;
        private readonly IRepositoryService _repositoryService;

        public ObjectController(IObjectService objectService, IRepositoryService repositoryService)
        {
            _objectService = objectService;
            _repositoryService = repositoryService;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrObjectList))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> List([FromRoute] string repositoryId, [FromQuery] StoragrObjectListQuery listArgs)
        {
            if (!await _repositoryService.Exists(repositoryId))
                return Error<RepositoryNotFoundError>();
            
            var count = await _objectService.Count(repositoryId);
            if (count == 0)
                return Ok<StoragrObjectList>();
            
            var list = (await _objectService.GetAll(repositoryId))
                .Select(v => (StoragrObject) v)
                .ToList();

            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.ObjectId != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? Ok<StoragrObjectList>()
                : Ok(new StoragrObjectList()
                {
                    Items = list,
                    NextCursor = list.Last().ObjectId,
                    TotalCount = count
                });
        }

        [HttpGet("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrObject))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Get([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!await _repositoryService.Exists(repositoryId))
                return Error<RepositoryNotFoundError>();

            try
            {
                return Ok(await _objectService.Get(repositoryId, objectId));
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
        }
        
        [HttpPost("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(StoragrError))]
        public async Task<IActionResult> Verify([FromRoute] string repositoryId, [FromRoute] string objectId, [FromBody] StoragrObject expectedObject)
        {
            if (objectId != expectedObject.ObjectId)
                return BadRequest();
            
            if (!await _repositoryService.Exists(repositoryId)) 
                return Error<RepositoryNotFoundError>();

            try
            {
                await _objectService.Add(repositoryId, expectedObject);
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
            return Ok();
        }

        [HttpDelete("{objectId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Delete([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!await _repositoryService.Exists(repositoryId)) 
                return Error<RepositoryNotFoundError>();
            
            if (!await _objectService.Exists(repositoryId, objectId)) 
                return Error<ObjectNotFoundError>();
            
            try
            {
                await _objectService.Delete(repositoryId, objectId);
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
            return NoContent();
        }
    }
}