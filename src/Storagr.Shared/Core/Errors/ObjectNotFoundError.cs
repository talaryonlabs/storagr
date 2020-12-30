using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr.Shared
{
    [DataContract]
    public class ObjectNotFoundError : StoragrError
    {
        public ObjectNotFoundError() : base(StatusCodes.Status404NotFound, "Object not found.")
        {
            
        }
    }
}