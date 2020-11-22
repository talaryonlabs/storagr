using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared.Data;
using Storagr.Store.Services;

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
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task Download([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            if (!_storeService.Exists(repositoryId, objectId))
            {
                Response.StatusCode = 404;
                Response.ContentLength = 0;
                return;
            }
            Response.ContentLength = _storeService.Get(repositoryId, objectId).Size;

            await using var stream = _storeService.GetDownloadStream(repositoryId, objectId);
            await stream.CopyToAsync(Response.Body, _storeService.BufferSize);
        }

        [HttpPut]
        [Consumes("application/octet-stream")]
        [ProducesResponseType(200)]
        public async Task Upload([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            await using var stream = _storeService.GetUploadStream(repositoryId, objectId);
            await Request.Body.CopyToAsync(stream, _storeService.BufferSize);
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult Finish([FromRoute] string repositoryId, [FromRoute] string objectId, [FromBody] StoreObject verifyObject)
        {
            return !_storeService.FinalizeUpload(verifyObject.RepositoryId, verifyObject.ObjectId, verifyObject.Size) ? StatusCode(500) : Ok();
        }
    }
}