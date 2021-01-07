using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("users")]
    public class UserController : StoragrController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrUser> ViewMe(CancellationToken cancellationToken)
        {
            return await _userService.GetAuthenticatedUser(cancellationToken);
        }
        
        [HttpPatch("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<ActionResult> UpdateMe()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUserList))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrUserList> List([FromQuery] StoragrUserListArgs listArgs, CancellationToken cancellationToken)
        {
            var count = await _userService.Count(cancellationToken: cancellationToken);
            if (count == 0)
                return new StoragrUserList();

            var list = (
                    string.IsNullOrEmpty(listArgs.Username)
                        ? await _userService.GetAll(cancellationToken)
                        : await _userService.GetMany(listArgs.Username, cancellationToken)
                )
                .Select(v => (StoragrUser) v)
                .ToList();

            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.UserId != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? new StoragrUserList()
                : new StoragrUserList()
                {
                    Items = list,
                    NextCursor = list.Last().UserId,
                    TotalCount = count
                };
        }


        [HttpPost]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(NotImplementedError))]
        public async Task<StoragrUser> Create([FromBody] StoragrUserRequest createRequest, CancellationToken cancellationToken)
        {
            // TODO!
                // await _userService.Create(createRequest.User.Username, createRequest.NewPassword,
                //     createRequest.User.IsAdmin);

            return null;
        }

        [HttpGet("{userId}")]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrUser> View([FromRoute] string userId, CancellationToken cancellationToken)
        {
            return await _userService.Get(userId, cancellationToken);
        }
        
        [HttpPatch("{userId}")] 
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrUser> Modify([FromRoute] string userId, [FromBody] StoragrUserRequest modifyRequest, CancellationToken cancellationToken)
        {
            // TODO!
            // return await _userService.Modify(modifyRequest.User, modifyRequest.NewPassword);

            return null;
        }
        
        [HttpDelete("{userId}")]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(NotImplementedError))]
        public async Task<StoragrUser> Delete([FromRoute] string userId, CancellationToken cancellationToken)
        {
            return await _userService.Delete(userId, cancellationToken);
        }
        
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrAuthenticationResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(UnauthorizedError))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(UsernameOrPasswordMissingError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrAuthenticationResponse> Authenticate([FromBody] StoragrAuthenticationRequest authenticationRequest, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(authenticationRequest.Username) || string.IsNullOrEmpty(authenticationRequest.Password))
                throw new UsernameOrPasswordMissingError();

            var user = await _userService.Authenticate(authenticationRequest.Username, authenticationRequest.Password, cancellationToken);
            
            return new StoragrAuthenticationResponse()
            {
                Token = user.Token
            };
        }
    }
}