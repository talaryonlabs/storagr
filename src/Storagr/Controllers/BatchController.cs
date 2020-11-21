using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost()]
        [ProducesResponseType(200, Type = typeof(BatchResponse))]
        [ProducesResponseType(403, Type = typeof(StoragrError))]
        [ProducesResponseType(404, Type = typeof(RepositoryNotFoundError))]
        [ProducesResponseType(422, Type = typeof(StoragrError))]
        [ProducesResponseType(500, Type = typeof(StoragrError))]
        public async Task<IActionResult> Batch([FromRoute] string rid, [FromBody] BatchRequest request)
        {
            var repository = await _objectService.Get(rid);
            if (repository == null)
                return NotFound(new RepositoryNotFoundError());

            return request.Operation switch
            {
                BatchOperation.Download => await Download(repository, request),
                BatchOperation.Upload => await Upload(repository, request),
                _ => StatusCode(500, new InvalidBatchOperationError())
            };
        }

        [NonAction]
        private async Task<IActionResult> Download(RepositoryEntity repository, BatchRequest batchRequest)
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
                    return new BatchResponseModel()
                    {
                        ObjectId = v.ObjectId,
                        Size = v.Size,
                        Error = new BatchError()
                        {
                            Code = 404,
                            Message = "Object not found."
                        }
                    };
                }

                var request = await _objectService.NewDownloadRequest(repository.RepositoryId, v.ObjectId);
                return new BatchResponseModel()
                {
                    ObjectId = v.ObjectId,
                    Size = v.Size,
                    Authenticated = true,
                    Actions = new BatchActionList()
                    {
                        Download = StoragrConverter.ToBatchAction(request)
                    }
                };
            });
            return Ok(new BatchResponse()
            {
                Transfers = new[] {"basic"},
                Objects = await Task.WhenAll(responseObjectsAsync)
            });
        }

        [NonAction]
        private async Task<IActionResult> Upload(RepositoryEntity repository, BatchRequest batchRequest)
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
                    return new BatchResponseModel()
                    {
                        ObjectId = v.ObjectId,
                        Size = v.Size
                    };
                }

                var uploadRequest = await _objectService.NewUploadRequest(repository.RepositoryId, v.ObjectId);
                var verifyRequest = await _objectService.NewVerifyRequest(repository.RepositoryId, v.ObjectId);
                return new BatchResponseModel()
                {
                    ObjectId = v.ObjectId,
                    Size = v.Size,
                    Authenticated = true,
                    Actions = new BatchActionList()
                    {
                        Upload = StoragrConverter.ToBatchAction(uploadRequest),
                        Verify = StoragrConverter.ToBatchAction(verifyRequest),
                    }
                };
            });
            return Ok(new BatchResponse()
            {
                Transfers = new[] {"basic"},
                Objects = await Task.WhenAll(responseObjectsAsync.ToList())
            });
        }
    }
}