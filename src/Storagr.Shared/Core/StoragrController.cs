using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Shared
{
    public class StoragrController : ControllerBase
    {
        [Pure]
        [NonAction]
        protected static ObjectResult Ok<T>()
            where T : class, new()
        {
            return new OkObjectResult(
                Activator.CreateInstance<T>()
            );
        }
        
        [Pure]
        [NonAction]
        protected static ObjectResult Error<T>()
            where T : StoragrError
        {
            var error = Activator.CreateInstance<T>();
            return new ObjectResult(error)
            {
                StatusCode = error.Code
            };
        }

        [Pure]
        [NonAction]
        protected static ObjectResult Error(StoragrError error) =>
            new(error)
            {
                StatusCode = error.Code
            };

        [Pure]
        [NonAction]
        protected static ObjectResult Error(Exception exception) => 
            Error(new StoragrError(exception));
    }
}