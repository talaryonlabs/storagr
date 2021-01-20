using System;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrLock
    {
        [JsonProperty("id")] public string LockId { get; set; }
        [JsonProperty("path")] public string Path { get; set; }
        [JsonProperty("locked_at")] public DateTime LockedAt { get; set; }
        [JsonProperty("owner")] public StoragrOwner Owner { get; set; }

        public static implicit operator StoragrLock(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLock>(data);
    }

    [JsonObject]
    public class StoragrLockRequest
    {
        [JsonProperty("path", Required = Required.Always)] public string Path { get; set; }
        [JsonProperty("ref")] public StoragrRef Ref { get; set; }
    }

    [JsonObject]
    public class StoragrLockResponse
    {
        [JsonProperty("lock")] public StoragrLock Lock { get; set; }
        
        public static implicit operator StoragrLockResponse(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLockResponse>(data);
    }
    
    [JsonObject]
    public class StoragrUnlockRequest
    {
        [JsonProperty("force")] public bool Force { get; set; }
        [JsonProperty("ref")] public StoragrRef Ref { get; set; }
    }

    [JsonObject]
    public class StoragrUnlockResponse
    {
        [JsonProperty("lock")] public StoragrLock Lock { get; set; }
        
        public static implicit operator StoragrUnlockResponse(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrUnlockResponse>(data);
    }
}