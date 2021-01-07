using System.Runtime.Serialization;

namespace Storagr.Shared
{
    [DataContract]
    public sealed class ObjectNotFoundError : NotFoundError
    {
        public ObjectNotFoundError() 
            : base("Object not found.")
        {
            
        }
    }
}