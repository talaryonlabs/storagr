using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Storagr.Server.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("users")]
    public class UserController : StoragrController
    {
        private readonly IStoragrService _storagrService;
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger, IStoragrService storagrService)
        {
            _logger = logger;
            _storagrService = storagrService;
        }
        
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrUser> ViewMe(CancellationToken cancellationToken)
        {
            return await _storagrService.GetAuthenticatedUser().RunAsync(cancellationToken);
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
            var count = await _storagrService
                .Users()
                .Count()
                .RunAsync(cancellationToken);
            
            if (count == 0)
                return new StoragrUserList();

            ;


            var users = await _storagrService
                .Users()
                .Where(userParams =>
                {
                    if (!string.IsNullOrEmpty(listArgs.Username))
                        userParams.Username(listArgs.Username);
                })
                .RunAsync(cancellationToken);
            
            var list = users.Items
                .Select(v => (StoragrUser) v)
                .ToList();

            if (listArgs.Skip > 0)
                list = list.Skip(listArgs.Skip).ToList();
            
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
        public async Task<StoragrUser> Create([FromBody] StoragrRequest<StoragrUser> createRequest, CancellationToken cancellationToken)
        {
            var user = new StoragrUser();
            var password = (string) (createRequest.Items.ContainsKey("password")
                    ? createRequest.Items["password"]
                    : null
                );

            var newUser = user + createRequest;

            return await _storagrService
                .User((string)createRequest.Items["username"])
                .Create()
                .With(u =>
                {
                    u
                        .Password((string)createRequest.Items.FirstOrDefault(v => v.Key == "password").Value)
                        .IsAdmin((bool) createRequest.Items["is_admin"])
                        .IsEnabled((bool) createRequest.Items["is_enabled"]);
                })
                .RunAsync(cancellationToken);
        }

        [HttpGet("{userId}")]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrUser> View([FromRoute] string userId, CancellationToken cancellationToken)
        {
            return await _storagrService
                .User(userId)
                .RunAsync(cancellationToken);
        }
        
        [HttpPatch("{userId}")] 
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrUser> Update([FromRoute] string userId, [FromBody] StoragrRequest<StoragrUser> updateRequest, CancellationToken cancellationToken)
        {
            var newPassword = default(string);
            var user = await _storagrService
                .User(userId)
                .RunAsync(cancellationToken);
            
            if (updateRequest.Items.ContainsKey("password"))
                newPassword = (string) updateRequest.Items["password"];

            var updatedUser = user + updateRequest;
            
            return await _storagrService
                .User(userId)
                .Update()
                .With(u =>
                    {
                        // TODO
                    }
                )
                .RunAsync(cancellationToken);
        }
        
        [HttpDelete("{userId}")]
        [Authorize(Policy = StoragrConstants.ManagementPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(NotImplementedError))]
        public async Task<StoragrUser> Delete([FromRoute] string userId, CancellationToken cancellationToken)
        {
            return await _storagrService
                .User(userId)
                .Delete()
                .RunAsync(cancellationToken);
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

            _logger.LogInformation("hallo du!");

            var user = await _storagrService
                .Authenticate(authenticationRequest.Username, authenticationRequest.Password)
                .RunAsync(cancellationToken);
            
            return new StoragrAuthenticationResponse()
            {
                Token = user.Token
            };
        }
    }
}