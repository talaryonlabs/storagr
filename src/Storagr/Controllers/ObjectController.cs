using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Client.Models;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.IO;
using Storagr.Services;

namespace Storagr.Controllers
{
    [ApiController]
    [Authorize]
    [Route("{rid}/objects")]
    public class ObjectController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IObjectService _objectService;
        private readonly IBackendAdapter _backend;
        private readonly IStoreAdapter _store;

        public ObjectController(IBackendAdapter backend, IUserService userService, IStoreAdapter store, IObjectService objectService)
        {
            _backend = backend;
            _userService = userService;
            _store = store;
            _objectService = objectService;
        }
        
        
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ObjectListResponse))]
        [ProducesResponseType(404, Type = typeof(StoragrError))]
        public async Task<IActionResult> List([FromRoute] string rid, [FromQuery] ObjectListRequest request)
        {
            var repository = await _backend.Get<RepositoryEntity>(rid);
            if (repository == null)
                return NotFound(new RepositoryNotFoundError());
            
            var user = await _userService.GetAuthenticatedUser();
            var objects = (await _objectService.GetAll(rid));

            var list = objects.ToList();
            if (!list.Any())
                return Ok(new ObjectListResponse() {Objects = new ObjectModel[0]});

            if (!string.IsNullOrEmpty(request.Cursor))
                list = list.SkipWhile(v => v.ObjectId != request.Cursor).ToList();

            if (request.Limit > 0)
                list = list.Take(request.Limit).ToList();

            return Ok(new ObjectListResponse()
            {
                Objects = list.Select(v => new ObjectModel()
                {
                    ObjectId = v.ObjectId,
                    Size = v.Size,
                    RepositoryId = v.RepositoryId
                }),
                NextCursor = list.Last().ObjectId
            });
        }

        [HttpGet("o/{oid}")]
        [ProducesResponseType(200, Type = typeof(ObjectModel))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get([FromRoute] string rid, [FromRoute] string oid)
        {
            var obj = await _objectService.Get(rid, oid);
            if (obj == null)
                return NotFound();

            return Ok((ObjectModel)obj);
        }
        
        [HttpPost("v/{oid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Verify([FromRoute] string rid, [FromRoute] string oid, [FromQuery] ObjectVerifyRequest verifyRequest)
        {
            if (oid != verifyRequest.ObjectId)
                return BadRequest();
            
            var obj = await _objectService.Get(rid, oid);
            if (obj == null)
                return NotFound();

            return Ok((ObjectModel)obj);
        }

        [HttpDelete("o/{oid}")]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute] string rid, [FromRoute] string oid)
        {
            try
            {
                await _store.DeleteObject(rid, oid);
                await _objectService.Delete(rid, oid);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500);
            }
            return NoContent();
        }
    }
}