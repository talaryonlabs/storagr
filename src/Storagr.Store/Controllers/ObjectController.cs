using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Store.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiRoute("objects")]
    public class ObjectController : StoragrController
    {
        private readonly IStoreService _storeService;

        public ObjectController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StoreObject>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public IEnumerable<StoreObject> List([FromQuery] ObjectControllerQuery query) => _storeService
            .Repository(query.RepositoryId)
            .Objects()
            .Select(obj => obj.Model());

        [HttpGet("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StoreObject))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public StoreObject Get([FromRoute] string objectId, [FromQuery] ObjectControllerQuery query) => _storeService
            .Repository(query.RepositoryId)
            .Object(objectId)
            .Model();
        
        [HttpDelete("{objectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
        public IActionResult Delete([FromRoute] string objectId, [FromQuery] ObjectControllerQuery query)
        {
            _storeService
                .Repository(query.RepositoryId)
                .Object(objectId)
                .Delete();

            return Ok();
        }
    }
    
    [DataContract]
    public class ObjectControllerQuery
    {
        [QueryMember("r")] public string RepositoryId { get; set; }
    }
}