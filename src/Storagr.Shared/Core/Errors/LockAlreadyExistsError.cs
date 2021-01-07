using System.Runtime.Serialization;
using Storagr.Shared.Data;

namespace Storagr.Shared
{
    [DataContract]
    public sealed class LockAlreadyExistsError : ConflictError
    {
        [DataMember(Name = "lock")] public StoragrLock Lock;
        
        public LockAlreadyExistsError(StoragrLock existingLock)
            : base("Lock already exists.")
        {
            Lock = existingLock;
        }
    }
}