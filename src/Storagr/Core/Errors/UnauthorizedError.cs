using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr
{
    [DataContract]
    public class UnauthorizedError : StoragrError
    {
        public UnauthorizedError(string message) 
            : base(StatusCodes.Status401Unauthorized, message)
        {
        }
    }
}