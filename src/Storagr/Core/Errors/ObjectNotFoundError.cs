using System.Runtime.Serialization;

namespace Storagr
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