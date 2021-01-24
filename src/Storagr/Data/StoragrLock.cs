using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Storagr.Data
{
    [JsonObject]
    public class StoragrLock
    {
        [JsonProperty("id")] public string LockId { get; set; }
        [JsonProperty("path")] public string Path { get; set; }
        [JsonProperty("locked_at")] public DateTime LockedAt { get; set; }
        [JsonProperty("owner")] public StoragrOwner Owner { get; set; }
    }

    [JsonObject]
    public class StoragrLockRequest
    {
        [JsonProperty("path", Required = Required.Always)]
        public string Path { get; set; }

        [JsonProperty("ref")] public StoragrRef Ref { get; set; }
    }

    [JsonObject]
    public class StoragrLockResponse
    {
        [JsonProperty("lock")] public StoragrLock Lock { get; set; }
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
    }

    [JsonObject]
    public class StoragrLockList : StoragrList<StoragrLock>
    {
        [JsonProperty("locks")] 
        public override IEnumerable<StoragrLock> Items { get; set; } = new List<StoragrLock>();
    }

    [DataContract]
    public sealed class StoragrLockListArgs : StoragrListArgs
    {
        [QueryMember("path")] public string Path { get; set; }
        [QueryMember("id")] public string Id { get; set; }

        [QueryMember("refspec")] public string RefSpec { get; set; }
    }

    [JsonObject]
    public sealed class StoragrLockVerifyList
    {
        [JsonProperty("ours")] public IEnumerable<StoragrLock> Ours { get; set; } = new List<StoragrLock>();
        [JsonProperty("theirs")] public IEnumerable<StoragrLock> Theirs { get; set; } = new List<StoragrLock>();
        [JsonProperty("next_cursor")] public string NextCursor { get; set; }
        [JsonProperty("total_count")] public int TotalCount { get; set; }
    }

    [DataContract]
    public sealed class StoragrLockVerifyListArgs : StoragrListArgs
    {
        // [JsonProperty("ref")] public StoragrRef Ref { get; set; }
    }
}