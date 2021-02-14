using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("{repositoryId}/locks")]
    public class LockController : StoragrController
    {
        private readonly ILockService _lockService;
        private readonly IUserService _userService;
        private readonly IRepositoryService _repositoryService;

        public LockController(ILockService lockService, IUserService userService, IRepositoryService repositoryService)
        {
            _lockService = lockService;
            _userService = userService;
            _repositoryService = repositoryService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLockList))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RepositoryNotFoundError))]
        public async Task<IActionResult> ListLocks([FromRoute] string repositoryId, [FromQuery] StoragrLockListArgs listArgs)
        {
            // TODO only if authorized -> Forbidden!
            // TODO consider the "refspec" property in request
            if (!await _repositoryService.Exists(repositoryId))
                return Error<RepositoryNotFoundError>();

            var user = await _userService.GetAuthenticatedUser();
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repositoryId, user.Id, RepositoryAccessType.Read))
                return Error<ForbiddenError>();

            var count = await _lockService.Count(repositoryId);
            if (count == 0)
                return Ok<StoragrUserList>();

            var list = (
                    string.IsNullOrEmpty(listArgs.LockId) && string.IsNullOrEmpty(listArgs.Path)
                        ? await _lockService.GetAll(repositoryId)
                        : await _lockService.GetAll(repositoryId, listArgs)
                )
                .Select(v => (StoragrLock) v)
                .ToList();

            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.LockId != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? Ok<StoragrLockList>()
                : Ok(new StoragrLockList()
                {
                    Items = list,
                    NextCursor = list.Last().LockId,
                    TotalCount = count
                });
        }

        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLockVerifyList))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RepositoryNotFoundError))]
        public async Task<IActionResult> VerifyLocks([FromRoute] string repositoryId, [FromBody] StoragrLockVerifyListArgs listArgs)
        {
            if (!await _repositoryService.Exists(repositoryId))
                return Error<RepositoryNotFoundError>();

            var user = await _userService.GetAuthenticatedUser();
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repositoryId, user.Id, RepositoryAccessType.Write))
                return Error<ForbiddenError>();
            
            var count = await _lockService.Count(repositoryId);
            if (count == 0)
                return Ok<StoragrUserList>();
            
            var list = (await _lockService.GetAll(repositoryId))
                .ToList();

            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.Id != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? Ok<StoragrLockVerifyList>()
                : Ok(new StoragrLockVerifyList()
                {
                    Ours = list.Where(v => v.OwnerId == user.Id).Select(v => (StoragrLock) v).ToList(),
                    Theirs = list.Where(v => v.OwnerId != user.Id).Select(v => (StoragrLock) v).ToList(),
                    NextCursor = list.Last().Id,
                    TotalCount = count
                });
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(StoragrLockResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(RepositoryNotFoundError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(LockAlreadyExistsError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StoragrError))]
        public async Task<IActionResult> CreateLock([FromRoute] string repositoryId, [FromBody] StoragrLockRequest lockRequest)
        {
            // TODO consider "ref" property in request

            if (!await _repositoryService.Exists(repositoryId))
                return Error<RepositoryNotFoundError>();
            
            var user = await _userService.GetAuthenticatedUser();
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repositoryId, user.Id, RepositoryAccessType.Write))
                return Error<ForbiddenError>();

            if (await _lockService.ExistsByPath(repositoryId, lockRequest.Path))
            {
                return Error(new LockAlreadyExistsError(
                    await _lockService.GetByPath(repositoryId, lockRequest.Path)
                ));
            }

            var newLock = new LockEntity()
            {
                Id = StoragrHelper.UUID(),
                LockedAt = DateTime.Now,
                OwnerId = user.Id,
                Path = lockRequest.Path,
                RepositoryId = repositoryId
            };
            try
            {
                await _lockService.Create(repositoryId, newLock);
                return Created(
                    $"v1/repositories/{repositoryId}/locks/{newLock.Id}",
                    newLock
                );
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
        }

        [HttpPost("{lockId}/unlock")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLock))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> DeleteLock([FromRoute] string repositoryId, [FromRoute] string lockId, [FromBody] StoragrUnlockRequest unlockRequest)
        {
            // TODO consider "ref" property in request

            if (!await _repositoryService.Exists(repositoryId))
                return Error<RepositoryNotFoundError>();
            
            var user = await _userService.GetAuthenticatedUser();
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repositoryId, user.Id, RepositoryAccessType.Write))
                return Error<ForbiddenError>();

            if (!await _lockService.Exists(repositoryId, lockId))
                return Error<LockNotFoundError>();

            var lockObj = await _lockService.Get(repositoryId, lockId);
            if (!unlockRequest.Force && lockObj.OwnerId != user.Id) // only delete own locks OR with force=true argument
                return Error<ForbiddenError>();

            try
            {
                await _lockService.Delete(repositoryId, lockId);
                return Ok(new StoragrUnlockResponse()
                {
                    Lock = lockObj
                });
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
        }
    }
}