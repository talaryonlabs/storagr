using System.Runtime.Serialization;
using Storagr.Shared.Data;

namespace Storagr.Shared
{
    [DataContract]
    public sealed class ObjectAlreadyExistsError : ConflictError
    {
        [DataMember(Name = "object")] public StoragrObject Object;
        
        public ObjectAlreadyExistsError(StoragrObject existingObject) 
            : base("Object already exists.")
        {
            Object = existingObject;
        }
    }
}