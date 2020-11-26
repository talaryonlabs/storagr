using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Controllers
{
    [ApiController]
    [Authorize]
    [Route("{repositoryId}/transfer/{objectId}")]
    public class TransferController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public TransferController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        [Produces("application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task Download([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!_storeService.Exists(repositoryId, objectId))
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                Response.ContentLength = 0;
                return;
            }
            Response.ContentLength = _storeService.Get(repositoryId, objectId).Size;

            await using var stream = _storeService.GetDownloadStream(repositoryId, objectId);
            await stream.CopyToAsync(Response.Body, _storeService.BufferSize);
        }

        [HttpPut]
        [Consumes("application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task Upload([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            try
            {
                await using var stream = _storeService.GetUploadStream(repositoryId, objectId);
                await Request.Body.CopyToAsync(stream, _storeService.BufferSize);
            }
            catch (ObjectAlreadyExistsException)
            {
                Response.StatusCode = StatusCodes.Status409Conflict;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public IActionResult Finish([FromRoute] string repositoryId, [FromRoute] string objectId, [FromBody] StoreObject verifyObject)
        {
            if (!_storeService.Exists(repositoryId))
                return (ActionResult) new RepositoryNotFoundError();
            
            return !_storeService.FinalizeUpload(verifyObject.RepositoryId, verifyObject.ObjectId, verifyObject.Size) ? StatusCode(500) : Ok();
        }
    }
}