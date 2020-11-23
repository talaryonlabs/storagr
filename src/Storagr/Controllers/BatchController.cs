using System.Linq;
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
    [ApiController]
    [Authorize]
    [Route("{rid}/objects/batch")]
    public class BatchController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IObjectService _objectService;

        public BatchController(IUserService userService, IObjectService objectService)
        {
            _userService = userService;
            _objectService = objectService;
        }
        
        
        [HttpGet]
        [ProducesResponseType(200, Type=typeof(string))]
        public IActionResult Default()
        {
            return Ok("Batch API");
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrBatchResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StoragrError))]
        public async Task<IActionResult> Batch([FromRoute] string rid, [FromBody] StoragrBatchRequest request)
        {
            var repository = await _objectService.Get(rid);
            if (repository == null)
                return (ActionResult) new RepositoryNotFoundError();

            return request.Operation switch
            {
                StoragrBatchOperation.Download => await Download(repository, request),
                StoragrBatchOperation.Upload => await Upload(repository, request),
                _ => (ActionResult) new NotImplementedError()
            };
        }

        [NonAction]
        private async Task<IActionResult> Download(RepositoryEntity repository, StoragrBatchRequest batchRequest)
        {
            // TODO consider the "ref" property in request
            // TODO check if user has read access

            var requestObjects = batchRequest.Objects.ToList();
            var objects =
                (await _objectService.GetMany(repository.RepositoryId,
                    requestObjects.Select(v => v.ObjectId).ToArray())).ToList();

            var responseObjectsAsync = requestObjects.Select(async v =>
            {
                if (!objects.Exists(x => x.ObjectId == v.ObjectId))
                {
                    return new StoragrBatchResponseObject()
                    {
                        ObjectId = v.ObjectId,
                        Size = v.Size,
                        Error = new ObjectNotFoundError()
                    };
                }

                return new StoragrBatchResponseObject()
                {
                    ObjectId = v.ObjectId,
                    Size = v.Size,
                    Authenticated = true,
                    Actions = new StoragrActions()
                    {
                        Download = await _objectService.NewDownloadAction(repository.RepositoryId, v.ObjectId)
                    }
                };
            });
            return Ok(new StoragrBatchResponse()
            {
                Transfers = new[] {"basic"},
                Objects = await Task.WhenAll(responseObjectsAsync)
            });
        }

        [NonAction]
        private async Task<IActionResult> Upload(RepositoryEntity repository, StoragrBatchRequest batchRequest)
        {
            // TODO consider the "ref" property in request
            // TODO check if user has write access

            var requestObjects = batchRequest.Objects.ToList();
            var objects =
                (await _objectService.GetMany(repository.RepositoryId,
                    requestObjects.Select(v => v.ObjectId).ToArray())).ToList();

            var token = await _userService.GetAuthenticatedUserToken();
            var responseObjectsAsync = requestObjects.Select(async v =>
            {
                if (objects.Exists(x => x.ObjectId == v.ObjectId))
                {
                    return new StoragrBatchResponseObject()
                    {
                        ObjectId = v.ObjectId,
                        Size = v.Size
                    };
                }

                return new StoragrBatchResponseObject()
                {
                    ObjectId = v.ObjectId,
                    Size = v.Size,
                    Authenticated = true,
                    Actions = new StoragrActions()
                    {
                        Upload = await _objectService.NewUploadAction(repository.RepositoryId, v.ObjectId),
                        Verify = await _objectService.NewVerifyAction(repository.RepositoryId, v.ObjectId),
                    }
                };
            });
            return Ok(new StoragrBatchResponse()
            {
                Transfers = new[] {"basic"},
                Objects = await Task.WhenAll(responseObjectsAsync.ToList())
            });
        }
    }
}