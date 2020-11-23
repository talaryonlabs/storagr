using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.Services;
using Storagr.Shared;
using Storagr.Shared.Data;

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
        [ProducesResponseType(typeof(StoragrLockListResponse),200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(StoragrError), 500)]
        public async Task<IActionResult> ListLocks([FromRoute] string rid, [FromQuery] StoragrLockListRequest request)
        {
            // TODO only if authorized
            // TODO only if read access
            // TODO consider the "refspec" property in request
            
            var repository = await _backend.Get<RepositoryEntity>(rid);
            if (repository == null)
                return NotFound(new RepositoryNotFoundError());

            var locks = await _lockService.GetAll(rid, request.Limit, request.Cursor, request.LockId, request.Path);
            var list = locks.ToList();
            
            return Ok(new StoragrLockListResponse()
            {
                Locks = list.Select(v => (StoragrLock)v).ToList(),
                NextCursor = list.LastOrDefault()?.LockId
            });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyLocks([FromRoute] string rid, [FromBody] StoragrLockVerifyListRequest request)
        {
            // TODO only if write access
            
            var repository = await _backend.Get<RepositoryEntity>(rid);
            if (repository == null)
                return NotFound(new RepositoryNotFoundError());

            var user = await _userService.GetAuthenticatedUser();
            var locks = await _lockService.GetAll(rid, request.Limit, request.Cursor);
            var list = locks.ToList();

            return Ok(new StoragrLockVerifyListResponse()
            {
                Ours = list.Where(v => v.OwnerId == user.UserId).Select(v => (StoragrLock)v).ToList(),
                Theirs = list.Where(v => v.OwnerId != user.UserId).Select(v => (StoragrLock)v).ToList(),
                NextCursor = list.LastOrDefault()?.LockId
            });
        }
        
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(409)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateLock([FromRoute] string rid, [FromBody] StoragrLockRequest lockRequest)
        {
            // TODO only if authorized
            // TODO consider "ref" property in request
            
            var repository = await _backend.Get<RepositoryEntity>(rid);
            if (repository == null)
                return NotFound(new RepositoryNotFoundError());
            
            
            LockEntity lockEntity;

            var cacheKey = $"LOCK:{rid}:{lockRequest.Path}";
            var cacheData = await _cache.GetAsync(cacheKey);
            if (cacheData != null)
            {
                await _cache.RefreshAsync(cacheKey);
                return Conflict(new LockAlreadyExistsError(StoragrHelper.DeserializeObject<StoragrLock>(cacheData)));
            }

            if ((lockEntity = await _lockService.GetByPath(rid, lockRequest.Path)) != null)
            {
                return Conflict(new LockAlreadyExistsError(lockEntity));
            }

            if ((lockEntity = await _lockService.Create(rid, lockRequest.Path)) == null)
            {
                return StatusCode(500, new StoragrError());
            }

            var obj = (StoragrLock) lockEntity;
            {
                await _cache.SetAsync($"LOCK:{rid}:{obj.Path}", (cacheData = StoragrHelper.SerializeObject(obj)), _cacheEntryOptions);
                await _cache.SetAsync($"LOCK:{rid}:{obj.LockId}", cacheData, _cacheEntryOptions);
            }
            return Created("", new StoragrLockResponse() { Lock = obj });
            
        }

        [HttpPost("{id}/unlock")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteLock([FromRoute] string rid, [FromRoute] string id, [FromBody] StoragrLockUnlockRequest unlockRequest)
        {
            // TODO only if authorized
            // TODO only if write access
            // TODO consider "ref" property in request

            var repository = await _backend.Get<RepositoryEntity>(rid);
            if (repository == null)
                return NotFound(new RepositoryNotFoundError());
            
            StoragrLock obj = default;
            
            var cacheData = await _cache.GetAsync($"LOCK:{rid}:{id}");
            if (cacheData != null)
            {
                obj = StoragrHelper.DeserializeObject<StoragrLock>(cacheData);
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
                return StatusCode(500, new StoragrError()
                {
                    Message = "Lock with id " + id + " not found."
                });

            var user = await _userService.GetAuthenticatedUser();
            if (!unlockRequest.Force && obj.Owner.Name != user.Username) // only delete own locks OR with force=true argument
                return Forbid();
            
            await _cache.RemoveAsync($"LOCK:{rid}:{obj.LockId}");
            await _cache.RemoveAsync($"LOCK:{rid}:{obj.Path}");
            await _lockService.Delete(rid, id);
            
            return Ok(new StoragrLockUnlockResponse()
            {
                Lock = obj
            });
        }
    }
}