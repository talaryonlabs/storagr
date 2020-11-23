using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Security;
using Storagr.Security.Authenticators;
using Storagr.Services;
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
        [ProducesResponseType(200, Type = typeof(IEnumerable<StoragrUser>))]
        public async Task<IActionResult> List() =>
            Ok((await _userService.GetAll()).Select(v => (StoragrUser) v));

        [HttpPost]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(201)]
        [ProducesResponseType(501)]
        public async Task<IActionResult> Create([FromBody] StoragrUserRequest createRequest)
        {
            if (!(_authentication is BackendAuthenticator authenticator)) return StatusCode(501);
            
            await authenticator.Create(createRequest.Username, createRequest.Password, createRequest.Mail, createRequest.Role);
            return Created("", null);

        }
        
        [HttpGet("me")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> Test()
        {
            var user = await _userService.GetAuthenticatedUser();
            
            return Ok($"Hello {user.Username}!");
        }
        
        [HttpGet("{userId}")]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(200, Type = typeof(StoragrUser))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> View([FromRoute] string userId)
        {
            var user = await _userService.Get(userId);
            if (user == null)
                return NotFound();

            return Ok((StoragrUser) user);
        }
        
        [HttpPatch("{userId}")] 
        [Authorize(Policy = "Management")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Modify([FromRoute] string userId, [FromBody] StoragrUserRequest modifyRequest)
        {
            var user = await _userService.Get(userId);
            if (user == null)
                return NotFound();
            
            // TODO await _userService.Modify(entity);
            
            return Ok();
        }
        
        [HttpDelete("{userId}")]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete([FromRoute] string userId)
        {
            var user = await _userService.Get(userId);
            if (user == null)
                return NotFound();
            
            await _userService.Delete(userId);
            return Ok();
        }
        
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(200, Type = typeof(StoragrAuthenticationResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Authenticate([FromBody] StoragrAuthenticationRequest authenticationRequest)
        {
            if (string.IsNullOrEmpty(authenticationRequest.Username) || string.IsNullOrEmpty(authenticationRequest.Password))
                return BadRequest();

            var user = await _userService.Authenticate(authenticationRequest.Username, authenticationRequest.Password);
            if (user == null)
                return Unauthorized();

            return Ok(new StoragrAuthenticationResponse()
            {
                Token = user.Token
            });
        }
    }
}