using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr
{
    [DataContract]
    public class ConflictError : StoragrError
    {
        public ConflictError(string message) 
            : base(StatusCodes.Status409Conflict, message)
        {
        }
    }
}