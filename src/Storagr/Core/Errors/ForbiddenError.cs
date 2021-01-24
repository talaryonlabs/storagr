using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr
{
    [DataContract]
    public class ForbiddenError : StoragrError
    {
        public ForbiddenError() 
            : base(StatusCodes.Status403Forbidden, "Forbidden.")
        {
        }
    }
}