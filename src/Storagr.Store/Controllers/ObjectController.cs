using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("objects")]
    public class ObjectController : StoragrController
    {
        private readonly IStoreService _storeService;
        private readonly ILogger<ObjectController> _logger;

        public ObjectController(IStoreService storeService, ILogger<ObjectController> logger)
        {
            _storeService = storeService;
            _logger = logger;
        }

        /**
         * List
         */
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StoreObject>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public IEnumerable<StoreObject> List() => _storeService.Objects();
        
        /**
         * Meta
         */
        [HttpHead("{objectId}")]
        [ResponseCache(NoStore = true)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public void Head([FromRoute] string objectId)
        {
            try
            {
                if (_storeService.Object(objectId).Exists)
                {
                    Response.StatusCode = StatusCodes.Status200OK;
                    Response.ContentLength = _storeService
                        .Object(objectId)
                        .Size;
                }
                else
                {
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    Response.ContentLength = 0;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        /**
         * Download
         */
        [HttpGet("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreObject))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task Download([FromRoute] string objectId, CancellationToken cancellationToken)
        {
            try
            {
                var obj = _storeService.Object(objectId);

                Response.ContentLength = obj.Size;

                await using var stream = _storeService
                    .Object(objectId)
                    .GetStream();
                await stream.CopyToAsync(Response.Body, _storeService.BufferSize, cancellationToken);
            }
            catch (NotFoundError)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
            }
            catch (Exception e)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(e.Message);
            }
        }
        
        /**
         * Upload
         */
        [HttpPut("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreObject))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task Upload([FromRoute] string objectId, CancellationToken cancellationToken)
        {
            Response.StatusCode = StatusCodes.Status200OK;
            try
            {
                if (_storeService.Object(objectId).Exists)
                    throw new ConflictError("Object already exists!");

                await using var stream = _storeService
                    .Object(objectId)
                    .GetStream();
                await Request.Body.CopyToAsync(stream, _storeService.BufferSize, cancellationToken);
            }
            catch (ConflictError)
            {
                Response.StatusCode = StatusCodes.Status409Conflict;
            }
            catch (Exception e)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(e.Message);
            }
        }
        
        /**
         * Delete
         */
        [HttpDelete("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public IActionResult Delete([FromRoute] string objectId)
        {
            _storeService
                .Object(objectId)
                .Delete();

            return Ok();
        }
    }
}