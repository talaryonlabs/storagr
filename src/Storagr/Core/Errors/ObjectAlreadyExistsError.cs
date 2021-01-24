using System.Runtime.Serialization;
using Storagr.Data;

namespace Storagr
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