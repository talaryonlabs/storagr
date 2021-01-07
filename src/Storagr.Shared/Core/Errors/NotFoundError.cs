using Microsoft.AspNetCore.Http;

namespace Storagr.Shared
{
    public class NotFoundError : StoragrError
    {
        public NotFoundError(string message) 
            : base(StatusCodes.Status404NotFound, message)
        {
            
        }
    }
}