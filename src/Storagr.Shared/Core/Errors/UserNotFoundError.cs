using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr.Shared
{
    [DataContract]
    public class UserNotFoundError : StoragrError
    {
        public UserNotFoundError() : base(StatusCodes.Status404NotFound, "User not found.")
        {
        }
    }
}