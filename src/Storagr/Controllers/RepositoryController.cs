using System;
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
    [ApiVersion("1.0")]
    [ApiRoute("repositories")]
    [Authorize(Policy = StoragrConstants.ManagementPolicy)]
    public class RepositoryController : StoragrController
    {
        private readonly IRepositoryService _repositoryService;

        public RepositoryController(IRepositoryService repositoryService)
        {
            _repositoryService = repositoryService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrRepositoryList))]
        public async Task<IActionResult> List([FromQuery] StoragrRepositoryListArgs listArgs)
        {
            var count = await _repositoryService.Count();
            if (count == 0)
                return Ok<StoragrRepositoryList>();

            var list = (string.IsNullOrEmpty(listArgs.Id)
                    ? await _repositoryService.GetAll()
                    : await _repositoryService.GetMany(listArgs.Id))
                .Select(v => (StoragrRepository) v)
                .ToList();

            if (!string.IsNullOrEmpty(listArgs.Cursor))
                list = list.SkipWhile(v => v.RepositoryId != listArgs.Cursor).Skip(1).ToList();

            list = list.Take(listArgs.Limit > 0
                ? Math.Max(listArgs.Limit, StoragrConstants.MaxListLimit)
                : StoragrConstants.DefaultListLimit).ToList();

            return !list.Any()
                ? Ok<StoragrRepositoryList>()
                : Ok(new StoragrRepositoryList()
                {
                    Items = list,
                    NextCursor = list.Last().RepositoryId,
                    TotalCount = count
                });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(StoragrRepository))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(StoragrError))]
        public async Task<IActionResult> Create([FromBody] StoragrRepository newRepository)
        {
            if (await _repositoryService.Exists(newRepository.RepositoryId))
            {
                return Error(new RepositoryAlreadyExistsError(
                    await _repositoryService.Get(newRepository.RepositoryId)
                ));
            }

            try
            {
                return Created(
                    $"v1/repositories/{newRepository.RepositoryId}",
                    await _repositoryService.Create(newRepository)
                );
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
        }

        [HttpGet("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoragrRepository))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Get([FromRoute] string repositoryId)
        {
            try
            {
                return Ok(await _repositoryService.Get(repositoryId));
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
        }

        [HttpDelete("{repositoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StoragrError))]
        public async Task<IActionResult> Delete([FromRoute] string repositoryId)
        {
            try
            {
                return Ok(await _repositoryService.Delete(repositoryId));
            }
            catch (Exception exception)
            {
                return Error(exception is StoragrError error ? error : exception);
            }
        }
    }
}