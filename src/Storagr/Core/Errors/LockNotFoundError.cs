using System.Runtime.Serialization;

namespace Storagr
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