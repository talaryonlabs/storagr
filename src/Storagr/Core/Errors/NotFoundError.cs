using Microsoft.AspNetCore.Http;

namespace Storagr
{
    public class NotFoundError : StoragrError
    {
        public NotFoundError(string message) 
            : base(StatusCodes.Status404NotFound, message)
        {
            
        }
    }
}