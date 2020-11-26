using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Security;
using Storagr.Security.Authenticators;
using Storagr.Services;
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
        private readonly IAuthenticationAdapter _authentication;

        public UserController(IUserService userService, IAuthenticationAdapter authentication)
        {
            _userService = userService;
            _authentication = authentication;
        }
        
        [HttpGet]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StoragrUser>))]
        public async Task<IActionResult> List() =>
            Ok((await _userService.GetAll()).Select(v => (StoragrUser) v));

        [HttpPost]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(StoragrError))]
        public async Task<IActionResult> Create([FromBody] StoragrUserRequest createRequest)
        {
            if (!(_authentication is BackendAuthenticator authenticator))
                return (ActionResult) new NotImplementedError();
            
            await authenticator.Create(createRequest.Username, createRequest.Password, createRequest.Mail, createRequest.Role);
            return Created("", null);

        }
        
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Test()
        {
            var user = await _userService.GetAuthenticatedUser();
            
            return Ok($"Hello {user.Username}!");
        }
        
        [HttpGet("{userId}")]
        [Authorize(Policy = "Management")]
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
        [Authorize(Policy = "Management")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Modify([FromRoute] string userId, [FromBody] StoragrUserRequest modifyRequest)
        {
            var user = await _userService.Get(userId);
            if (user == null)
                return (ActionResult) new UserNotFoundError();
            
            // TODO await _userService.Modify(entity);
            
            return Ok();
        }
        
        [HttpDelete("{userId}")]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Delete([FromRoute] string userId)
        {
            var user = await _userService.Get(userId);
            if (user == null)
                return (ActionResult) new UserNotFoundError();

            await _userService.Delete(userId);
            return NoContent();
        }
        
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrAuthenticationResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StoragrError))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(StoragrError))]
        public async Task<IActionResult> Authenticate([FromBody] StoragrAuthenticationRequest authenticationRequest)
        {
            if (string.IsNullOrEmpty(authenticationRequest.Username) || string.IsNullOrEmpty(authenticationRequest.Password))
                return (ActionResult) new UsernameOrPasswordMissingError();

            var user = await _userService.Authenticate(authenticationRequest.Username, authenticationRequest.Password);
            if (user == null)
                return (ActionResult) new AuthenticationError();
            
            return Ok(new StoragrAuthenticationResponse()
            {
                Token = user.Token
            });
        }
    }
}