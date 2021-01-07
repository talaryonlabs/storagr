using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("{repositoryId}/transfer/{objectId}")]
    public class TransferController : StoragrController
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task Download([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            try
            {
                var obj = _storeService.Get(repositoryId, objectId);

                Response.ContentLength = obj.Size;

                await using var stream = _storeService.GetDownloadStream(obj.RepositoryId, obj.ObjectId);
                await stream.CopyToAsync(Response.Body, _storeService.BufferSize);
            }
            catch (NotFoundError)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
            }
            catch (Exception)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        [HttpPut]
        [Consumes("application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task Upload([FromRoute] string repositoryId, [FromRoute] string objectId)
        {
            Response.StatusCode = StatusCodes.Status200OK;
            try
            {
                await using var stream = _storeService.GetUploadStream(repositoryId, objectId);
                await Request.Body.CopyToAsync(stream, _storeService.BufferSize);
            }
            catch (NotFoundError)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
            }
            catch (ConflictError)
            {
                Response.StatusCode = StatusCodes.Status409Conflict;
            }
            catch (Exception)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public IActionResult Finish([FromRoute] string repositoryId, [FromRoute] string objectId, [FromBody] StoreObject verifyObject)
        {
            if (!_storeService.Exists(repositoryId))
                throw new RepositoryNotFoundError();

            _storeService.FinalizeUpload(verifyObject.RepositoryId, verifyObject.ObjectId, verifyObject.Size);
            
            return Ok();
        }
    }
}