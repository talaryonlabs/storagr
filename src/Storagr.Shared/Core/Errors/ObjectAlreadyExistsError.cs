using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Storagr.Shared.Data;

namespace Storagr.Shared
{
    [DataContract]
    public class ObjectAlreadyExistsError : StoragrError
    {
        [DataMember(Name = "object")] public StoragrObject Object;
        
        public ObjectAlreadyExistsError(StoragrObject existingObject) : base(StatusCodes.Status409Conflict, "Object already exists.")
        {
            Object = existingObject;
        }
    }
}