using System;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared;

namespace Storagr
{
    public class StoragrController : ControllerBase
    {
        [NonAction]
        protected ObjectResult Ok<T>()
            where T : class, new()
        {
            return new OkObjectResult(
                Activator.CreateInstance<T>()
            );
        }
        
        [NonAction]
        protected ObjectResult Error<T>()
            where T : StoragrError
        {
            var error = Activator.CreateInstance<T>();
            return new ObjectResult(error)
            {
                StatusCode = error.Code
            };
        }

        [NonAction]
        protected ObjectResult Error(StoragrError error) => new ObjectResult(error)
        {
            StatusCode = error.Code
        };

        [NonAction]
        protected ObjectResult Error(Exception exception) => 
            Error(new StoragrError(exception));
    }
}