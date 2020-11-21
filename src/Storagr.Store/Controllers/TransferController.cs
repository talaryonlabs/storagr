using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // TODO move this to ObjectService and StoreService
        /*[HttpPost("{oid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Verify([FromRoute] string rid, [FromRoute] string oid, [FromBody] StoreVerifyRequest verifyRequest)
        {
            if (oid != verifyRequest.ObjectId)
                return BadRequest();

            var user = await _userService.GetAuthenticatedUser();
            var key = $"TMPFILE:{rid}:{oid}";
            var name = await _cache.GetStringAsync(key);
            if (name == null)
            {
                return NotFound();
            }
            await _cache.RemoveAsync(key);
            
            var file = _store.GetTemporaryFile(name);
            if (file == null)
                return NotFound();

            var size = file.Length;
            if (size != verifyRequest.Size)
            {
                file.Delete();
                return BadRequest();
            }
            _store.Save(file, rid, oid);
        }*/
    }
}