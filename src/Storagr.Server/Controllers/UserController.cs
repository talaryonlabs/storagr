using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Storagr.Shared;
using Storagr.Shared.Data;

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
            return await _storagrService
                .Authorization()
                .GetAuthenticatedUser()
                .RunAsync(cancellationToken);
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

            var list = (
                await _storagrService
                    .Users()
                    .Skip(listArgs.Skip)
                    .SkipUntil(listArgs.Cursor)
                    .Take(listArgs.Limit)
                    .Where(userParams => userParams
                        .Id(listArgs.Id)
                        .Username(listArgs.Username)
                        .IsAdmin(listArgs.IsAdmin)
                        .IsEnabled(listArgs.IsEnabled)
                    )
                    .RunAsync(cancellationToken)
            ).ToList();
            
            return !list.Any()
                ? new StoragrUserList()
                : new StoragrUserList()
                {
                    Items = list.Select(v => (StoragrUser) v),
                    NextCursor = list.Last().Id,
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

            var user = await _storagrService
                .Authorization()
                .Authenticate()
                .With(authenticationRequest.Username, authenticationRequest.Password)
                .RunAsync(cancellationToken);

            return new StoragrAuthenticationResponse()
            {
                Token = user.Token
            };
        }
    }
}