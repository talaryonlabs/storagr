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
    [ApiController]
    [Authorize]
    [Route("users")]
    public class UserController : ControllerBase
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
                return (ActionResult) new NotImplementedError();
            }
            catch (UserAlreadyExistsException)
            {
                return (ActionResult) new UserAlreadyExistsError();
            }
            catch (Exception e)
            {
                return (ActionResult) new StoragrError(e.Message);
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
            if (user == null)
                return (ActionResult) new UserNotFoundError();

            return Ok((StoragrUser) user);
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
                return (ActionResult) new UserNotFoundError();
            }
            catch (Exception e)
            {
                return (ActionResult) new StoragrError(e.Message);
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
                return (ActionResult) new NotImplementedError();
            }
            catch (UserNotFoundException)
            {
                return (ActionResult) new UserNotFoundError();   
            }
            catch (Exception e)
            {
                return (ActionResult) new StoragrError(e.Message);
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
                return (ActionResult) new UsernameOrPasswordMissingError();

            UserEntity user;
            try
            {
                user = await _userService.Authenticate(authenticationRequest.Username, authenticationRequest.Password);
                if (user == null)
                    return (ActionResult) new AuthenticationError();
            }
            catch (Exception e)
            {
                return (ActionResult) new StoragrError(e);
            }
            
            return Ok(new StoragrAuthenticationResponse()
            {
                Token = user.Token
            });
        }
    }
}