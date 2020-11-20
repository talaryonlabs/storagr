﻿using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Storagr.Controllers.Models;
using Storagr.Data.Entities;
using Storagr.Security;
using Storagr.Security.Authenticators;
using Storagr.Services;

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

        [AllowAnonymous]
        [HttpPost("login")]
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
        
        [HttpGet("logout")]
        [ProducesResponseType(200)]
        public IActionResult Logout()
        {
            // TODO logout current user
            
            return Ok();
        }


        [HttpGet]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(200, Type = typeof(UserListResponse))]
        public async Task<IActionResult> List()
        {
            var list = await _userService.GetAll();
            return Ok(new UserListResponse()
            {
                Users = list.Select(v => (UserModel) v).ToList()
            });
        }
        
        [HttpGet("u/{uid}")]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(200, Type = typeof(UserModel))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> View([FromRoute] string uid)
        {
            var user = await _userService.Get(uid);
            if (user == null)
                return NotFound();

            return Ok((UserModel) user);
        }

        [HttpPost("create")]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(201)]
        [ProducesResponseType(501)]
        public async Task<IActionResult> Create([FromBody] UserCreateRequest createRequest)
        {
            if (!(_authentication is BackendAuthenticator authenticator)) return StatusCode(501);
            
            await authenticator.Create(createRequest.Username, createRequest.Password, createRequest.Mail, createRequest.Role);
            return Created("", null);

        }
        
        [HttpPatch("u/{uid}")] 
        [Authorize(Policy = "Management")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Modify([FromRoute] string uid, [FromBody] UserModifyRequest modifyRequest)
        {
            var user = await _userService.Get(uid);
            if (user == null)
                return NotFound();

            if (_authentication is BackendAuthenticator authenticator)
            {
                await authenticator.Modify(user.AuthId, modifyRequest.Username, modifyRequest.Password, modifyRequest.Mail, modifyRequest.Role);
            }
            // TODO modify profile
            // await _userService.Modify(entity);
            
            return Ok();
        }
        
        [HttpDelete("u/{uid}")]
        [Authorize(Policy = "Management")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete([FromRoute] string uid)
        {
            var user = await _userService.Get(uid);
            if (user == null)
                return NotFound();
            
            if (_authentication is BackendAuthenticator authenticator)
                await authenticator.Delete(uid);
            
            await _userService.Delete(uid);
            return Ok();
        }
    }
}