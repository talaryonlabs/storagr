using System;
using System.Collections.Generic;
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
    [ApiRoute("users")]
    public class UserController : StoragrController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ViewMe()
        {
            return Ok(
                await _userService.GetAuthenticatedUser()
            );
        }
        
        [HttpPatch("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateMe()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUserList))]
        public async Task<IActionResult> List([FromQuery] StoragrUserListArgs listArgs)
        {
            var count = await _userService.Count();
            if (count == 0)
                return Ok<StoragrUserList>();

            var list = (
                    string.IsNullOrEmpty(listArgs.Username)
                        ? await _userService.GetAll()
                        : await _userService.GetMany(listArgs.Username)
                )
                .Select(v => (StoragrUser) v)
                .ToList();

            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.UserId != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? Ok<StoragrUserList>()
                : Ok(new StoragrUserList()
                {
                    Items = list,
                    NextCursor = list.Last().UserId,
                    TotalCount = count
                });
        }


        [HttpPost]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(UserAlreadyExistsError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(NotImplementedError))]
        public async Task<IActionResult> Create([FromBody] StoragrUserRequest createRequest)
        {
            // TODO!
            try
            {
                // await _userService.Create(createRequest.User.Username, createRequest.NewPassword,
                //     createRequest.User.IsAdmin);
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
            
            return Created("", null);
        }

        [HttpGet("{userId}")]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> View([FromRoute] string userId)
        {
            if (!await _userService.Exists(userId))
                return Error<UserNotFoundError>();

            return Ok(
                (StoragrUser) await _userService.Get(userId)
            );
        }
        
        [HttpPatch("{userId}")] 
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(UserNotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StoragrError))]
        public async Task<IActionResult> Modify([FromRoute] string userId, [FromBody] StoragrUserRequest modifyRequest)
        {
            // TODO!
            try
            {
                // await _userService.Modify(modifyRequest.User, modifyRequest.NewPassword);
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
            
            return Ok();
        }
        
        [HttpDelete("{userId}")]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(UserNotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(NotImplementedError))]
        public async Task<IActionResult> Delete([FromRoute] string userId)
        {
            try
            {
                await _userService.Delete(userId);
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }

            return NoContent();
        }
        
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrAuthenticationResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthenticationError))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(UsernameOrPasswordMissingError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StoragrError))]
        public async Task<IActionResult> Authenticate([FromBody] StoragrAuthenticationRequest authenticationRequest)
        {
            if (string.IsNullOrEmpty(authenticationRequest.Username) || string.IsNullOrEmpty(authenticationRequest.Password))
                return Error<UsernameOrPasswordMissingError>();

            try
            {
                var user = await _userService.Authenticate(authenticationRequest.Username, authenticationRequest.Password);
                if (user is null)
                    return Error<AuthenticationError>();
                
                return Ok(new StoragrAuthenticationResponse()
                {
                    Token = user.Token
                });
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
        }
    }
}