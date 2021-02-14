using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("repositories/{repositoryId}/objects")]
    [Authorize(Policy = StoragrConstants.ManagementPolicy)]
    public class ObjectController : StoragrController
    {
        private readonly IStoragrService _storagrService;
        private readonly IBatchService _batchService;

        public ObjectController(IStoragrService storagrService, IBatchService batchService)
        {
            _storagrService = storagrService;
            _batchService = batchService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrObjectList))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrObjectList> List([FromRoute] string repositoryId, [FromQuery] StoragrObjectListArgs listArgs,
            CancellationToken cancellationToken)
        {
            var repository = _storagrService
                .Repository(repositoryId);

            if (!await repository.Exists().RunAsync(cancellationToken))
                throw new RepositoryNotFoundError();

            var count = await repository
                .Objects()
                .Count()
                .RunAsync(cancellationToken);
            if (count == 0)
                return new StoragrObjectList();

            var list = (
                await repository
                    .Objects()
                    .Skip(listArgs.Skip)
                    .SkipUntil(listArgs.Cursor)
                    .Take(listArgs.Limit)
                    .RunAsync(cancellationToken)
            ).ToList();

            return !list.Any()
                ? new StoragrObjectList()
                : new StoragrObjectList()
                {
                    Items = list.Select(v => (StoragrObject) v),
                    NextCursor = list.Last().Id,
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
            // TODO
            // if (objectId != expectedObject.ObjectId)
            //     throw new BadRequestError();
            //
            // if (!await _repositoryService.Exists(repositoryId, cancellationToken))
            //     throw new RepositoryNotFoundError();
            //
            // return await _objectService.Add(repositoryId, expectedObject, cancellationToken);
            return null;
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
        
        [HttpPost("batch")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrBatchResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrBatchResponse> Batch([FromRoute] string repositoryId, [FromBody] StoragrBatchRequest request, CancellationToken cancellationToken)
        {
            var operation = request.Operation switch
            {
                StoragrBatchOperation.Download => _batchService
                    .Repository(repositoryId)
                    .Objects(request.Objects)
                    .Download(),
                StoragrBatchOperation.Upload => _batchService
                    .Repository(repositoryId)
                    .Objects(request.Objects)
                    .Upload(),
                _ => throw new ArgumentOutOfRangeException()
            };

            var objects = (await operation
                    .With(batchParams => batchParams
                        .Ref(request.Ref.Name)
                        .Transfers(request.Transfers)
                    )
                    .RunAsync(cancellationToken)
                )
                .ToList();

            if (objects.Count(v => v.Error is not null) == objects.Count)
            {
                throw new StoragrError(422, "Unable to process any item.");
            }
            
            return new StoragrBatchResponse()
            {
                TransferAdapter = StoragrConstants.DefaultTransferAdapter,
                Objects = objects
            };
        }
    }
}