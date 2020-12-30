using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Storagr.Shared.Data;

namespace Storagr.Shared
{
    [DataContract]
    public class LockAlreadyExistsError : StoragrError
    {
        [DataMember(Name = "lock")] public StoragrLock Lock;
        
        public LockAlreadyExistsError(StoragrLock existingLock) : base(StatusCodes.Status409Conflict, "Lock already exists.")
        {
            Lock = existingLock;
        }
    }
}