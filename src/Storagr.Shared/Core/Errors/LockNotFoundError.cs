using System.Runtime.Serialization;

namespace Storagr.Shared
{
    [DataContract]
    public sealed class LockNotFoundError : NotFoundError
    {
        public LockNotFoundError() 
            : base("Lock not found.")
        {
            
        }
    }
}