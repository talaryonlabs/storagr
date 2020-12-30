using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr.Shared
{
    [DataContract]
    public class LockNotFoundError : StoragrError
    {
        public LockNotFoundError() : base(StatusCodes.Status404NotFound, "Lock not found.")
        {
            
        }
    }
}