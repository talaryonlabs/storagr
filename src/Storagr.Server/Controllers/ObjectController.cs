using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Server.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("repositories/{repositoryId}/objects")]
    [Authorize(Policy = StoragrConstants.ManagementPolicy)]
    public class ObjectController : StoragrController
    {
        private readonly IStoragrService _storagrService;

        public ObjectController(IStoragrService storagrService)
        {
            _storagrService = storagrService;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrObjectList))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrObjectList> List([FromRoute] string repositoryId, [FromQuery] StoragrObjectListArgs listArgs, CancellationToken cancellationToken)
        {
            if (!await _repositoryService.Exists(repositoryId, cancellationToken))
                throw new RepositoryNotFoundError();
            
            var count = await _objectService.Count(repositoryId, cancellationToken);
            if (count == 0)
                return new StoragrObjectList();
            
            var list = (await _objectService.GetAll(repositoryId, cancellationToken))
                .Select(v => (StoragrObject) v)
                .ToList();

            if (listArgs.Skip > 0)
                list = list.Skip(listArgs.Skip).ToList();
            
            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.ObjectId != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? new StoragrObjectList()
                : new StoragrObjectList()
                {
                    Items = list,
                    NextCursor = list.Last().ObjectId,
                    TotalCount = count
                };
        }

        [HttpGet("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrObject))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrObject> Get([FromRoute] string repositoryId, [FromRoute] string objectId, CancellationToken cancellationToken)
        {
            return await _storagrService
                .Repository(repositoryId)
                .Object(objectId)
                .RunAsync(cancellationToken);
        }
        
        [HttpPost("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrObject> Verify([FromRoute] string repositoryId, [FromRoute] string objectId, [FromBody] StoragrObject expectedObject, CancellationToken cancellationToken)
        {
            if (objectId != expectedObject.ObjectId)
                throw new BadRequestError();

            if (!await _repositoryService.Exists(repositoryId, cancellationToken))
                throw new RepositoryNotFoundError();

            return await _objectService.Add(repositoryId, expectedObject, cancellationToken);
        }

        [HttpDelete("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrObject> Delete([FromRoute] string repositoryId, [FromRoute] string objectId, CancellationToken cancellationToken)
        {
            return await _storagrService
                .Repository(repositoryId)
                .Object(objectId)
                .Delete()
                .RunAsync(cancellationToken);
        }
    }
}