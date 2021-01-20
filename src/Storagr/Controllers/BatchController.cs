using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Data.Entities;
using Storagr.Services;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("repositories/{repositoryId}/objects/batch")]
    public class BatchController : StoragrController
    {
        private readonly IUserService _userService;
        private readonly IRepositoryService _repositoryService;
        private readonly IObjectService _objectService;

        public BatchController(IUserService userService, IObjectService objectService, IRepositoryService repositoryService)
        {
            _userService = userService;
            _objectService = objectService;
            _repositoryService = repositoryService;
        }
        
        
        [HttpGet]
        [ProducesResponseType(200, Type=typeof(string))]
        public IActionResult Default()
        {
            return Ok("Batch API");
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrBatchResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrBatchResponse> Batch([FromRoute] string repositoryId, [FromBody] StoragrBatchRequest request, CancellationToken cancellationToken)
        {
            var repository = await _repositoryService.Get(repositoryId, cancellationToken);
            return request.Operation switch
            {
                StoragrBatchOperation.Download => await Download(repository, request, cancellationToken),
                StoragrBatchOperation.Upload => await Upload(repository, request, cancellationToken),
                _ => throw new NotImplementedError()
            };
        }

        [NonAction]
        private async Task<StoragrBatchResponse> Download(RepositoryEntity repository, StoragrBatchRequest batchRequest, CancellationToken cancellationToken)
        {
            // TODO consider the "ref" property in request
            var user = await _userService.GetAuthenticatedUser(cancellationToken);
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repository.Id, user.Id, RepositoryAccessType.Read, cancellationToken))
                throw new ForbiddenError();

            var requestObjects = batchRequest.Objects.ToList();
            var objects =
                (await _objectService.GetMany(repository.Id,
                    requestObjects.Select(v => v.ObjectId), cancellationToken)).ToList();

            var responseObjectsAsync = requestObjects.Select(async v =>
            {
                if (!objects.Exists(x => x.Id == v.ObjectId))
                {
                    return new StoragrBatchObject()
                    {
                        ObjectId = v.ObjectId,
                        Size = v.Size,
                        Error = new ObjectNotFoundError()
                    };
                }

                return new StoragrBatchObject()
                {
                    ObjectId = v.ObjectId,
                    Size = v.Size,
                    Authenticated = true,
                    Actions = new StoragrActions()
                    {
                        Download = await _objectService.NewDownloadAction(repository.Id, v.ObjectId, cancellationToken)
                    }
                };
            });
            return new StoragrBatchResponse()
            {
                Transfers = new[] {"basic"},
                Objects = await Task.WhenAll(responseObjectsAsync)
            };
        }

        [NonAction]
        private async Task<StoragrBatchResponse> Upload(RepositoryEntity repository, StoragrBatchRequest batchRequest, CancellationToken cancellationToken)
        {
            // TODO consider the "ref" property in request
            var user = await _userService.GetAuthenticatedUser(cancellationToken);
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repository.Id, user.Id, RepositoryAccessType.Write, cancellationToken))
                throw new ForbiddenError();

            var requestObjects = batchRequest.Objects.ToList();
            var objects =
                (await _objectService.GetMany(repository.Id,
                    requestObjects.Select(v => v.ObjectId), cancellationToken)).ToList();

            var responseObjectsAsync = requestObjects.Select(async v =>
            {
                if (objects.Exists(x => x.Id == v.ObjectId))
                {
                    return new StoragrBatchObject()
                    {
                        ObjectId = v.ObjectId,
                        Size = v.Size
                    };
                }

                return new StoragrBatchObject()
                {
                    ObjectId = v.ObjectId,
                    Size = v.Size,
                    Authenticated = true,
                    Actions = new StoragrActions()
                    {
                        Upload = await _objectService.NewUploadAction(repository.Id, v.ObjectId, cancellationToken),
                        Verify = await _objectService.NewVerifyAction(repository.Id, v.ObjectId, cancellationToken),
                    }
                };
            });
            return new StoragrBatchResponse()
            {
                Transfers = new[] {"basic"},
                Objects = await Task.WhenAll(responseObjectsAsync.ToList())
            };
        }
    }
}