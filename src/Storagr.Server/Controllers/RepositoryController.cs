using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            var count = await _repositoryService.Count(cancellationToken: cancellationToken);
            if (count == 0)
                return new StoragrRepositoryList();

            var list = (string.IsNullOrEmpty(listArgs.Id)
                    ? await _repositoryService.GetAll(cancellationToken)
                    : await _repositoryService.GetMany(listArgs.Id, cancellationToken: cancellationToken))
                .Select(v => (StoragrRepository) v)
                .ToList();

            if (listArgs.Skip > 0)
                list = list.Skip(listArgs.Skip).ToList();

            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.RepositoryId != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? new StoragrRepositoryList()
                : new StoragrRepositoryList()
                {
                    Items = list,
                    NextCursor = list.Last().RepositoryId,
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