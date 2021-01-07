using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr.Shared
{
    [DataContract]
    public class UnauthorizedError : StoragrError
    {
        public UnauthorizedError() 
            : base(StatusCodes.Status401Unauthorized, "Authentication failed. Username and Password correct?")
        {
        }
    }
}