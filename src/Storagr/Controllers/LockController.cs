﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.Services;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("{rid}/locks")]
    public class LockController : StoragrController
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLockList))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RepositoryNotFoundError))]
        public async Task<IActionResult> ListLocks([FromRoute] string rid, [FromQuery] StoragrLockListQuery listQuery)
        {
            // TODO only if authorized -> Forbidden!
            // TODO consider the "refspec" property in request
            var repository = await _backend.Get<RepositoryEntity>(rid);
            if (repository == null)
                return Error<RepositoryNotFoundError>();
            
            if (!await _userService.HasAccess(repository, RepositoryAccessType.Read))
                return Error<ForbiddenError>();

            var locks = await _lockService.GetAll(rid, listQuery.Limit, listQuery.Cursor, listQuery.LockId, listQuery.Path);
            
            var list = locks.ToList();
            if (!list.Any())
                return Ok<StoragrLockList>();
            
            return Ok(new StoragrLockList()
            {
                Items = list.Select(v => (StoragrLock)v).ToList(),
                NextCursor = list.LastOrDefault()?.Id
            });
        }

        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLockVerifyList))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RepositoryNotFoundError))]
        public async Task<IActionResult> VerifyLocks([FromRoute] string rid, [FromBody] StoragrLockVerifyListRequest request)
        {
            var repository = await _backend.Get<RepositoryEntity>(rid);
            if (repository == null)
                return Error<RepositoryNotFoundError>();

            if (!await _userService.HasAccess(repository, RepositoryAccessType.Write))
                return Error<ForbiddenError>();

            var user = await _userService.GetAuthenticatedUser();
            var locks = await _lockService.GetAll(rid, request.Limit, request.Cursor);
            var list = locks.ToList();

            return Ok(new StoragrLockVerifyList()
            {
                Ours = list.Where(v => v.OwnerId == user.Id).Select(v => (StoragrLock)v).ToList(),
                Theirs = list.Where(v => v.OwnerId != user.Id).Select(v => (StoragrLock)v).ToList(),
                NextCursor = list.LastOrDefault()?.Id
            });
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(StoragrLockResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RepositoryNotFoundError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(LockAlreadyExistsError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StoragrError))]
        public async Task<IActionResult> CreateLock([FromRoute] string rid, [FromBody] StoragrLockRequest lockRequest)
        {
            // TODO consider "ref" property in request

            var repository = await _backend.Get<RepositoryEntity>(rid);
            if (repository == null)
                return Error<RepositoryNotFoundError>();
            
            if (!await _userService.HasAccess(repository, RepositoryAccessType.Write))
                return Error<ForbiddenError>();
            
            LockEntity lockEntity;

            var cacheKey = $"LOCK:{rid}:{lockRequest.Path}";
            var cacheData = await _cache.GetAsync(cacheKey);
            if (cacheData != null)
            {
                await _cache.RefreshAsync(cacheKey);
                return Error(new LockAlreadyExistsError(StoragrHelper.DeserializeObject<StoragrLock>(cacheData)));
            }

            if ((lockEntity = await _lockService.GetByPath(rid, lockRequest.Path)) != null)
            {
                return Error(new LockAlreadyExistsError(lockEntity));
            }

            if ((lockEntity = await _lockService.Create(rid, lockRequest.Path)) == null)
            {
                return Error<StoragrError>();
            }

            var obj = (StoragrLock) lockEntity;
            
            await _cache.SetAsync($"LOCK:{rid}:{obj.Path}", (cacheData = StoragrHelper.SerializeObject(obj)), _cacheEntryOptions);
            await _cache.SetAsync($"LOCK:{rid}:{obj.LockId}", cacheData, _cacheEntryOptions);
            
            return Created("", new StoragrLockResponse()
            {
                Lock = obj
            });
        }

        [HttpPost("{id}/unlock")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLock))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> DeleteLock([FromRoute] string rid, [FromRoute] string id, [FromBody] StoragrUnlockRequest unlockRequest)
        {
            // TODO consider "ref" property in request

            var repository = await _backend.Get<RepositoryEntity>(rid);
            if (repository == null)
                return Error<RepositoryNotFoundError>();

            if (!await _userService.HasAccess(repository, RepositoryAccessType.Write))
                return Error<ForbiddenError>();
            
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
                return Error<LockNotFoundError>();

            var user = await _userService.GetAuthenticatedUser();
            if (!unlockRequest.Force && obj.Owner.Name != user.Username) // only delete own locks OR with force=true argument
                return Error<ForbiddenError>();
            
            await _cache.RemoveAsync($"LOCK:{rid}:{obj.LockId}");
            await _cache.RemoveAsync($"LOCK:{rid}:{obj.Path}");
            await _lockService.Delete(rid, id);
            
            return Ok(new StoragrUnlockResponse()
            {
                Lock = obj
            });
        }
    }
}