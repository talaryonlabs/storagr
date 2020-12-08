using System;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLock
    {
        [DataMember(Name = "id")] public string LockId { get; set; }
        [DataMember(Name = "path")] public string Path { get; set; }
        [DataMember(Name = "locked_at")] public DateTime LockedAt { get; set; }
        [DataMember(Name = "owner")] public StoragrOwner Owner { get; set; }

        public static implicit operator StoragrLock(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLock>(data);
    }

    [DataContract]
    public class StoragrLockRequest
    {
        [DataMember(Name = "path", IsRequired = true)] public string Path { get; set; }
        [DataMember(Name = "ref")] public StoragrRef Ref { get; set; }
    }

    [DataContract]
    public class StoragrLockResponse
    {
        [DataMember(Name = "lock")] public StoragrLock Lock { get; set; }
        
        public static implicit operator StoragrLockResponse(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLockResponse>(data);
    }
    
    [DataContract]
    public class StoragrLockUnlockRequest
    {
        [DataMember(Name = "force")] public bool Force { get; set; }
        [DataMember(Name = "ref")] public StoragrRef Ref { get; set; }
    }

    [DataContract]
    public class StoragrLockUnlockResponse
    {
        [DataMember(Name = "lock")] public StoragrLock Lock { get; set; }
        
        public static implicit operator StoragrLockUnlockResponse(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLockUnlockResponse>(data);
    }
}