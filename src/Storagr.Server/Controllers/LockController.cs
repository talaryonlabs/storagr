using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Server.Data.Entities;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("repositories/{repositoryId}/locks")]
    public class LockController : StoragrController
    {
        private readonly IStoragrService _storagrService;

        public LockController(IStoragrService storagrService)
        {
            _storagrService = storagrService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLockList))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrLockList> ListLocks([FromRoute] string repositoryId, [FromQuery] StoragrLockListArgs listArgs,
            CancellationToken cancellationToken)
        {
            // TODO consider the "refspec" property in request
            var count = await _storagrService
                .Repository(repositoryId)
                .Locks()
                .Count()
                .RunAsync(cancellationToken);

            if (count == 0)
                return new StoragrLockList();

            var list = (
                await _storagrService
                    .Repository(repositoryId)
                    .Locks()
                    .Skip(listArgs.Skip)
                    .SkipUntil(listArgs.Cursor)
                    .Take(listArgs.Limit)
                    .Where(whereParams => whereParams
                        .Id(listArgs.Id)
                        .Path(listArgs.Id)
                    )
                    .RunAsync(cancellationToken)
            ).ToList();

            return !list.Any()
                ? new StoragrLockList()
                : new StoragrLockList()
                {
                    Items = list.Select(v => (StoragrLock) v),
                    NextCursor = list.Last().Id,
                    TotalCount = count
                };
        }

        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLockVerifyList))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrLockVerifyList> VerifyLocks([FromRoute] string repositoryId,
            [FromBody] StoragrLockVerifyListArgs listArgs, CancellationToken cancellationToken)
        {
            var user = await _storagrService
                .Authorization()
                .GetAuthenticatedUser()
                .RunAsync(cancellationToken);

            var access = user.IsAdmin || await _storagrService
                .Repository(repositoryId)
                .HasAccess(user.Id, RepositoryAccessType.Write)
                .RunAsync(cancellationToken);

            if (!access)
                throw new ForbiddenError();

            var count = await _storagrService
                .Repository(repositoryId)
                .Locks()
                .Count()
                .RunAsync(cancellationToken);

            if (count == 0)
                return new StoragrLockVerifyList();

            var list = (
                await _storagrService
                    .Repository(repositoryId)
                    .Locks()
                    .Skip(listArgs.Skip)
                    .SkipUntil(listArgs.Cursor)
                    .Take(listArgs.Limit)
                    .RunAsync(cancellationToken)
            ).ToList();

            return !list.Any()
                ? new StoragrLockVerifyList()
                : new StoragrLockVerifyList()
                {
                    Ours = list.Where(v => v.OwnerId == user.Id).Select(v => (StoragrLock) v),
                    Theirs = list.Where(v => v.OwnerId != user.Id).Select(v => (StoragrLock) v),
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
        public async Task<StoragrLockResponse> Lock([FromRoute] string repositoryId, [FromBody] StoragrLockRequest lockRequest,
            CancellationToken cancellationToken)
        {
            // TODO consider "ref" property in request
            return new()
            {
                Lock = await _storagrService
                    .Repository(repositoryId)
                    .Lock(lockRequest.Path)
                    .Create()
                    .RunAsync(cancellationToken)
            };
        }

        [HttpGet("{lockId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrLock))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrLock> View([FromRoute] string repositoryId, [FromRoute] string lockId,
            CancellationToken cancellationToken)
        {
            // TODO consider "ref" property in request
            return await _storagrService
                .Repository(repositoryId)
                .Lock(lockId)
                .RunAsync(cancellationToken);
        }

        [HttpPost("{lockId}/unlock")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUnlockResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrUnlockResponse> Unlock([FromRoute] string repositoryId, [FromRoute] string lockId,
            [FromBody] StoragrUnlockRequest unlockRequest, CancellationToken cancellationToken)
        {
            // TODO consider "ref" property in request
            return new()
            {
                Lock = await _storagrService
                    .Repository(repositoryId)
                    .Lock(lockId)
                    .Delete(unlockRequest.Force)
                    .RunAsync(cancellationToken)
            };
        }
    }
}