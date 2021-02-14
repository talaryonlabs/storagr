using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("transfer")]
    public class TransferController : StoragrController
    {
        private readonly IStoreService _storeService;
        private readonly ITransferService _transferService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TransferController> _logger;

        public TransferController(IStoreService storeService, ITransferService transferService, ILogger<TransferController> logger, IDistributedCache cache)
        {
            _storeService = storeService;
            _transferService = transferService;
            _logger = logger;
            _cache = cache;
        }

        [HttpPost]
        public StoreTransferResponse RequestTransfer([FromBody] StoreTransferRequest transferRequest)
        {
            var repository = _storeService
                .Repository(transferRequest.Repository.Id)
                .CreateIfNotExists()
                .SetName(transferRequest.Repository.Name);

            var objects = repository.Objects();

            return new StoreTransferResponse()
            {
                Objects = transferRequest.Type switch
                {
                    StoreTransferType.Download => objects
                        .Where(o => transferRequest.Objects.Contains(o.Id))
                        .Select(o => _transferService.AddRequest(repository.Id, o.Id)),

                    StoreTransferType.Upload => transferRequest.Objects
                        .Where(oid => objects.FirstOrDefault(v => v.Id == oid) is null)
                        .Select(oid => _transferService.AddRequest(repository.Id, oid)),

                    _ => throw new ArgumentOutOfRangeException()
                }
            };
        }

        [HttpGet("{transferId}")]
        [Produces("application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task Download([FromRoute] string transferId, CancellationToken cancellationToken)
        {
            try
            {
                var transferRequest = await _transferService.GetRequestAsync(transferId, cancellationToken) ??
                                      throw new NotFoundError("Request not found");
                
                var obj = _storeService
                    .Repository(transferRequest.RepositoryId)
                    .Object(transferRequest.ObjectId);

                Response.ContentLength = (long)obj.Size;

                await using var stream = obj.GetDownloadStream();
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

        [HttpPut("{transferId}")]
        [Consumes("application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task Upload([FromRoute] string transferId, CancellationToken cancellationToken)
        {
            Response.StatusCode = StatusCodes.Status200OK;
            try
            {
                var transferRequest = await _transferService.GetRequestAsync(transferId, cancellationToken) ??
                                      throw new NotFoundError("Request not found");
                
                await using var stream = _storeService
                    .Repository(transferRequest.RepositoryId)
                    .Object(transferRequest.ObjectId)
                    .GetUploadStream();
                
                await Request.Body.CopyToAsync(stream, _storeService.BufferSize, cancellationToken);
            }
            catch (NotFoundError)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
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

        [HttpPost("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public IActionResult Verify([FromRoute] string repositoryId, [FromRoute] string objectId, [FromBody] StoreObject verifyObject)
        {
            var obj = _storeService
                .Repository(repositoryId)
                .Object(objectId);

            if (!obj.Exists())
                throw new ObjectNotFoundError();

            if (obj.Size == verifyObject.Size) 
                return Ok();
            
            obj.Delete();
            throw new BadRequestError();
        }
    }
}