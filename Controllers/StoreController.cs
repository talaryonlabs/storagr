using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Controllers.Models;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.IO;
using Storagr.Services;

namespace Storagr.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("{rid}/store/")]
    public class StoreController : ControllerBase
    {
        private readonly LocalStore _store;
        private readonly IBackendAdapter _backend;
        private readonly IDistributedCache _cache;
        private readonly int _bufferSize;
        private readonly DistributedCacheEntryOptions _cacheEntryOptions;
        private readonly IUserService _userService;

        public StoreController(LocalStore store, IDistributedCache cache, IBackendAdapter backend, IUserService userService)
        {
            _store = store;
            _cache = cache;
            _backend = backend;
            _userService = userService;
            _bufferSize = 2048;
            _cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
        }
        
        
        [HttpGet("{oid}")]
        [Produces("application/octet-stream")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task Download([FromRoute] string rid, [FromRoute] string oid)
        {
            var obj = (LocalStoreObject)await _store.GetObject(rid, oid);
            if (obj == null)
            {
                Response.StatusCode = 404;
                Response.ContentLength = 0;
                return;
            }
            Response.ContentLength = obj.Size;

            await using var stream = obj.GetStream();
            await stream.CopyToAsync(Response.Body, _bufferSize);
        }

        [HttpPut("{oid}")]
        [Consumes("application/octet-stream")]
        [ProducesResponseType(200)]
        public async Task Upload([FromRoute] string rid, [FromRoute] string oid)
        {
            var key = $"TMPFILE:{rid}:{oid}";
            var name = await _cache.GetStringAsync(key);
            var file = name != null ? _store.GetTemporaryFile(name) : _store.CreateTemporaryFile();

            if (name == null)
            {
                await _cache.SetStringAsync(key, file.Name, _cacheEntryOptions);
            }
            await _cache.RefreshAsync(key);
            
            await using var stream = file.OpenWrite();
            await Request.Body.CopyToAsync(stream, _bufferSize);
        }

        [HttpPost("{oid}")]
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
            
            await _backend.Insert(new ObjectEntity()
            {
                RepositoryId = rid,
                ObjectId = oid,
                Size = size
            });
            
            return Ok();
        }
    }
}