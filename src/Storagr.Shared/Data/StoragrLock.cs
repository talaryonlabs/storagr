using System;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLock
    {
        [DataMember(Name = "id")] public string LockId;
        [DataMember(Name = "path")] public string Path;
        [DataMember(Name = "locked_at")] public DateTime LockedAt;
        [DataMember(Name = "owner")] public StoragrOwner Owner;
    }

    [DataContract]
    public class StoragrLockRequest
    {
        [DataMember(Name = "path", IsRequired = true)] public string Path;
        [DataMember(Name = "ref")] public StoragrRef Ref;
    }

    [DataContract]
    public class StoragrLockResponse
    {
        [DataMember(Name = "lock")] public StoragrLock Lock;
    }
    
    [DataContract]
    public class StoragrLockUnlockRequest
    {
        [DataMember(Name = "force")] public bool Force;
        [DataMember(Name = "ref")] public StoragrRef Ref;
    }

    [DataContract]
    public class StoragrLockUnlockResponse
    {
        [DataMember(Name = "lock")] public StoragrLock Lock;
    }
}