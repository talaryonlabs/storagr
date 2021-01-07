using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Storagr.Shared;

namespace Storagr
{
    public sealed class StoragrExceptionFilter : ExceptionFilterAttribute
    {
        private readonly StoragrMediaType _mediaType;

        public StoragrExceptionFilter(StoragrMediaType mediaType)
        {
            _mediaType = mediaType;
        }
        
        public override void OnException(ExceptionContext context)
        {
            var storagrError = context.Exception is StoragrError error
                ? error
                : new InternalServerError(context.Exception);
            
            context.Result = new ObjectResult(storagrError)
            {
                StatusCode = storagrError.Code
            };
            context.HttpContext.Response.StatusCode = storagrError.Code;
            context.HttpContext.Response.ContentType = _mediaType.MediaType.Value;
        }
    }
}