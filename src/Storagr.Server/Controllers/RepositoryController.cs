using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("repositories")]
    [Authorize(Policy = StoragrConstants.ManagementPolicy)]
    public class RepositoryController : StoragrController
    {
        private readonly IStoragrService _storagrService;
        public RepositoryController(IStoragrService storagrService)
        {
            _storagrService = storagrService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrRepositoryList))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrRepositoryList> List([FromQuery] StoragrRepositoryListArgs listArgs,
            CancellationToken cancellationToken)
        {
            var count = await _storagrService
                .Repositories()
                .Count()
                .RunAsync(cancellationToken);
            if (count == 0)
                return new StoragrRepositoryList();


            var list = (
                await _storagrService
                    .Repositories()
                    .Skip(listArgs.Skip)
                    .SkipUntil(listArgs.Cursor)
                    .Take(listArgs.Limit)
                    .Where(whereParams => whereParams
                        .Id(listArgs.Id)
                        .Name(listArgs.Name)
                        .Owner(listArgs.Owner)
                        .SizeLimit(listArgs.SizeLimit)
                    )
                    .RunAsync(cancellationToken)
            ).ToList();

            return !list.Any()
                ? new StoragrRepositoryList()
                : new StoragrRepositoryList()
                {
                    Items = list.Select(v => (StoragrRepository) v),
                    NextCursor = list.Last().Id,
                    TotalCount = count
                };
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrRepository))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrRepository> Create([FromBody] StoragrRequest<StoragrRepository> createRequest,
            CancellationToken cancellationToken)
            => await _storagrService
                .Repository((string)createRequest.Items["name"])
                .Create()
                .With(repoParams => repoParams
                    .Owner((string) createRequest.Items["owner"])
                    .SizeLimit((ulong) createRequest.Items["size_limit"])
                )
                .RunAsync(cancellationToken);

        [HttpGet("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrRepository))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrRepository> Get([FromRoute] string repositoryId, CancellationToken cancellationToken) =>
            await _storagrService
                .Repository(repositoryId)
                .RunAsync(cancellationToken);

        [HttpPatch("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrRepository))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrRepository> Update([FromRoute] string repositoryId,
            [FromBody] StoragrRequest<StoragrRepository> updateRequest, CancellationToken cancellationToken)
        {
            return await _storagrService
                .Repository(repositoryId)
                .Update()
                .With(repoParams =>
                {
                    if (updateRequest.Items.ContainsKey("name"))
                        repoParams.Name((string) updateRequest.Items["name"]);

                    if (updateRequest.Items.ContainsKey("owner"))
                        repoParams.Owner((string) updateRequest.Items["owner"]);

                    if (updateRequest.Items.ContainsKey("size_limit"))
                        repoParams.SizeLimit((ulong) updateRequest.Items["size_limit"]);
                })
                .RunAsync(cancellationToken);
        }

        [HttpDelete("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrRepository))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public async Task<StoragrRepository> Delete([FromRoute] string repositoryId, CancellationToken cancellationToken) =>
            await _storagrService
                .Repository(repositoryId)
                .Delete()
                .RunAsync(cancellationToken);
    }
}