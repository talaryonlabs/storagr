using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Services;
using Storagr.Shared.Data;

namespace Storagr.Controllers
{
    // TODO integrate to UserController => /users/authenticate
    [ApiController]
    [Authorize]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(UserLoginResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            if (string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
                return BadRequest();

            var user = await _userService.Authenticate(loginRequest.Username, loginRequest.Password);
            if (user == null)
                return Unauthorized();
            
            return Ok(new UserLoginResponse()
            {
                Token = user.Token
            });
        }
        
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult> Test()
        {
            var user = await _userService.GetAuthenticatedUser();
            
            return Ok($"Hello {user.Username}!");
        }
        
        [HttpGet("logout")]
        [ProducesResponseType(200)]
        public IActionResult Logout()
        {
            // TODO logout current user
            
            return Ok();
        }
    }
}