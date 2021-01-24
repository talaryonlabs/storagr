using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr
{
    [DataContract]
    public class BadRequestError : StoragrError
    {
        public BadRequestError() : base(StatusCodes.Status403Forbidden, "Forbidden.")
        {
        }
    }
}