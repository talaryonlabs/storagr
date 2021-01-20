using System;
using System.Linq;
using System.Threading;
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
    [ApiRoute("repositories/{repositoryId}/locks")]
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
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrLockList> ListLocks([FromRoute] string repositoryId, [FromQuery] StoragrLockListArgs listArgs, CancellationToken cancellationToken)
        {
            // TODO only if authorized -> Forbidden!
            // TODO consider the "refspec" property in request
            if (!await _repositoryService.Exists(repositoryId, cancellationToken))
                throw new RepositoryNotFoundError();

            var user = await _userService.GetAuthenticatedUser(cancellationToken);
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repositoryId, user.Id, RepositoryAccessType.Read, cancellationToken))
                throw new ForbiddenError();

            var count = await _lockService.Count(repositoryId, cancellationToken);
            if (count == 0)
                return new StoragrLockList();

            var list = (
                    string.IsNullOrEmpty(listArgs.Id) && string.IsNullOrEmpty(listArgs.Path)
                        ? await _lockService.GetAll(repositoryId, cancellationToken)
                        : await _lockService.GetMany(repositoryId, listArgs.Id, listArgs.Path, cancellationToken)
                )
                .Select(v => (StoragrLock) v)
                .ToList();

            if (listArgs.Skip > 0)
                list = list.Skip(listArgs.Skip).ToList();
            
            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.LockId != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? new StoragrLockList()
                : new StoragrLockList()
                {
                    Items = list,
                    NextCursor = list.Last().LockId,
                    TotalCount = count
                };
        }

        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLockVerifyList))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrLockVerifyList> VerifyLocks([FromRoute] string repositoryId, [FromBody] StoragrLockVerifyListArgs listArgs, CancellationToken cancellationToken)
        {
            if (!await _repositoryService.Exists(repositoryId, cancellationToken))
                throw new RepositoryNotFoundError();

            var user = await _userService.GetAuthenticatedUser(cancellationToken);
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repositoryId, user.Id, RepositoryAccessType.Write, cancellationToken))
                throw new ForbiddenError();
            
            var count = await _lockService.Count(repositoryId, cancellationToken);
            if (count == 0)
                return new StoragrLockVerifyList();
            
            var list = (await _lockService.GetAll(repositoryId, cancellationToken))
                .ToList();

            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.Id != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? new StoragrLockVerifyList()
                : new StoragrLockVerifyList()
                {
                    Ours = list.Where(v => v.OwnerId == user.Id).Select(v => (StoragrLock) v).ToList(),
                    Theirs = list.Where(v => v.OwnerId != user.Id).Select(v => (StoragrLock) v).ToList(),
                    NextCursor = list.Last().Id,
                    TotalCount = count
                };
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(StoragrLockResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrLockResponse> Lock([FromRoute] string repositoryId, [FromBody] StoragrLockRequest lockRequest, CancellationToken cancellationToken)
        {
            // TODO consider "ref" property in request

            if (!await _repositoryService.Exists(repositoryId, cancellationToken))
                throw new RepositoryNotFoundError();
            
            var user = await _userService.GetAuthenticatedUser(cancellationToken);
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repositoryId, user.Id, RepositoryAccessType.Write, cancellationToken))
                throw new ForbiddenError();

            if (await _lockService.ExistsByPath(repositoryId, lockRequest.Path, cancellationToken))
            {
                throw new LockAlreadyExistsError(
                    await _lockService.GetByPath(repositoryId, lockRequest.Path, cancellationToken)
                );
            }
            
            return new StoragrLockResponse()
            {
                Lock = await _lockService.Lock(repositoryId, lockRequest.Path, cancellationToken)
            };
        }

        [HttpPost("{lockId}/unlock")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUnlockResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrUnlockResponse> Unlock([FromRoute] string repositoryId, [FromRoute] string lockId, [FromBody] StoragrUnlockRequest unlockRequest, CancellationToken cancellationToken)
        {
            // TODO consider "ref" property in request

            if (!await _repositoryService.Exists(repositoryId, cancellationToken))
                throw new RepositoryNotFoundError();

            var user = await _userService.GetAuthenticatedUser(cancellationToken);
            if (!user.IsAdmin && !await _repositoryService.HasAccess(repositoryId, user.Id, RepositoryAccessType.Write, cancellationToken))
                throw new ForbiddenError();

            var lockObj = await _lockService.Get(repositoryId, lockId, cancellationToken);
            if (!unlockRequest.Force && lockObj.OwnerId != user.Id) // only delete own locks OR with force=true argument
                throw new ForbiddenError();

            return new StoragrUnlockResponse()
            {
                Lock = await _lockService.Unlock(repositoryId, lockId, cancellationToken)
            };
        }
    }
}