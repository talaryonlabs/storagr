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
        public async Task<ActionResult> Test()
        {
            var user = await _userService.GetAuthenticatedUser();

            return Ok(user);
        }
        
        [HttpGet]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StoragrUser>))]
        public async Task<IActionResult> List() =>
            Ok((await _userService.GetAll()).Select(v => (StoragrUser) v));

        [HttpPost]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(UserAlreadyExistsError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(NotImplementedError))]
        public async Task<IActionResult> Create([FromBody] StoragrUserRequest createRequest)
        {
            try
            {
                await _userService.Create(createRequest.User.Username, createRequest.NewPassword,
                    createRequest.User.IsAdmin);
            }
            catch (NotSupportedException)
            {
                return Error<NotImplementedError>();
            }
            catch (UserAlreadyExistsException)
            {
                return Error<UserAlreadyExistsError>();
            }
            catch (Exception e)
            {
                return Error(new StoragrError(e.Message));
            }
            
            return Created("", null);
        }

        [HttpGet("{userId}")]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> View([FromRoute] string userId)
        {
            var user = await _userService.Get(userId);
            return user == null ? Error<UserNotFoundError>() : Ok((StoragrUser) user);
        }
        
        [HttpPatch("{userId}")] 
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(UserNotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StoragrError))]
        public async Task<IActionResult> Modify([FromRoute] string userId, [FromBody] StoragrUserRequest modifyRequest)
        {
            try
            {
                await _userService.Modify(modifyRequest.User, modifyRequest.NewPassword);
            }
            catch (UserNotFoundException)
            {
                return Error<UserNotFoundError>();
            }
            catch (Exception e)
            {
                return Error(e);
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
            catch (NotSupportedException)
            {
                return Error<NotImplementedError>();
            }
            catch (UserNotFoundException)
            {
                return Error<UserNotFoundError>();
            }
            catch (Exception e)
            {
                return Error(e);
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

            UserEntity user;
            try
            {
                user = await _userService.Authenticate(authenticationRequest.Username, authenticationRequest.Password);
                if (user == null)
                    return Error<AuthenticationError>();
            }
            catch (Exception e)
            {
                return Error(e);
            }
            
            return Ok(new StoragrAuthenticationResponse()
            {
                Token = user.Token
            });
        }
    }
}