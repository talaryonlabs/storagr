using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Storagr.Controllers.Models;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.Services;

namespace Storagr.Controllers
{
    [ApiController]
    [Authorize]
    [Route("{rid}/locks")]
    public class LockController : ControllerBase
    {
        private readonly IBackendAdapter _backend;
        private readonly IDistributedCache _cache;
        private readonly ILockService _lockService;
        private readonly IUserService _userService;
        private readonly DistributedCacheEntryOptions _cacheEntryOptions;

        public LockController(IBackendAdapter backend, IDistributedCache cache, ILockService lockService, IUserService userService)
        {
            _backend = backend;
            _cache = cache;
            _lockService = lockService;
            _userService = userService;
            _cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(120));
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(LockListResponse),200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> ListLocks([FromRoute] string rid, [FromQuery] LockListRequest request)
        {
            // TODO only if authorized
            // TODO only if read access
            // TODO consider the "refspec" property in request

            var locks = await _lockService.GetAll(rid, request.Limit, request.Cursor, request.LockId, request.Path);
            var list = locks.ToList();
            
            return Ok(new LockListResponse()
            {
                Locks = list.Select(v => (LockModel)v).ToList(),
                NextCursor = list.Last()?.LockId
            });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyLocks([FromRoute] string rid, [FromBody] LockVerifyListRequest request)
        {
            // TODO only if write access

            var user = await _userService.GetAuthenticatedUser();
            var locks = await _lockService.GetAll(rid, request.Limit, request.Cursor);
            var list = locks.ToList();

            return Ok(new LockVerifyListResponse()
            {
                Ours = list.Where(v => v.OwnerId == user.UserId).Select(v => (LockModel)v).ToList(),
                Theirs = list.Where(v => v.OwnerId != user.UserId).Select(v => (LockModel)v).ToList(),
                NextCursor = list.Last()?.LockId
            });
        }
        
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(409)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateLock([FromRoute] string rid, [FromBody] LockRequest lockRequest)
        {
            // TODO only if authorized
            // TODO consider "ref" property in request
            LockEntity lockEntity;

            var cacheKey = $"LOCK:{rid}:{lockRequest.Path}";
            var cacheData = await _cache.GetAsync(cacheKey);
            if (cacheData != null)
            {
                await _cache.RefreshAsync(cacheKey);
                return Conflict(new LockAlreadyExistsError(StoragrHelper.DeserializeObject<LockModel>(cacheData)));
            }

            if ((lockEntity = await _lockService.GetByPath(rid, lockRequest.Path)) != null)
            {
                return Conflict(new LockAlreadyExistsError(lockEntity));
            }
            if ((lockEntity = await _lockService.Create(rid, lockRequest.Path)) == null)
            {
                return StatusCode(500, new Error());
            }

            var obj = (LockModel) lockEntity;
            {
                await _cache.SetAsync($"LOCK:{rid}:{obj.Path}", (cacheData = StoragrHelper.SerializeObject(obj)), _cacheEntryOptions);
                await _cache.SetAsync($"LOCK:{rid}:{obj.LockId}", cacheData, _cacheEntryOptions);
            }
            return Created("", new LockResponse() { Lock = obj }); // TODO uri?
            
        }

        [HttpPost("{id}/unlock")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteLock([FromRoute] string rid, [FromRoute] string id, [FromBody] UnlockRequest unlockRequest)
        {
            // TODO only if authorized
            // TODO only if write access
            // TODO only delete own locks OR with force=true argument > 403
            // TODO consider "ref" property in request

            LockModel obj = default;
            
            var cacheData = await _cache.GetAsync($"LOCK:{rid}:{id}");
            if (cacheData != null)
            {
                obj = StoragrHelper.DeserializeObject<LockModel>(cacheData);
            }
            if (obj == null)
            {
                var entity = await _lockService.Get(rid, id);
                if (entity != null)
                {
                    obj = entity;
                }
            }
            if (obj == null)
                return StatusCode(500, new Error()
                {
                    Message = "Lock with id " + id + " not found."
                });
            
            await _cache.RemoveAsync($"LOCK:{rid}:{obj.LockId}");
            await _cache.RemoveAsync($"LOCK:{rid}:{obj.Path}");
            await _lockService.Delete(rid, id);
            
            return Ok(new UnlockResponse()
            {
                Lock = obj
            });
        }
    }
}