using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr
{
    [DataContract]
    public class UsernameOrPasswordMissingError : StoragrError
    {
        public UsernameOrPasswordMissingError() : base(StatusCodes.Status422UnprocessableEntity, "Username or Password missing or empty!")
        {
        }
    }
}