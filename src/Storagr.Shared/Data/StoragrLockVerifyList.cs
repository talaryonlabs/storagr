using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public sealed class StoragrLockVerifyListArgs : IStoragrListArgs
    {
        [JsonProperty("cursor")] public string Cursor { get; set; }
        [JsonProperty("skip")] public int Skip { get; set; }
        [JsonProperty("limit")] public int Limit { get; set; }
        [JsonProperty("ref")] public StoragrRef Ref { get; set; }
    }
    
    [JsonObject]
    public sealed class StoragrLockVerifyList
    {
        [JsonProperty("ours")] public IEnumerable<StoragrLock> Ours { get; set; } = new List<StoragrLock>();
        [JsonProperty("theirs")] public IEnumerable<StoragrLock> Theirs { get; set; } = new List<StoragrLock>();
        [JsonProperty("next_cursor")] public string NextCursor { get; set; }
        [JsonProperty("total_count")] public int TotalCount { get; set; }
        
        public static implicit operator StoragrLockVerifyList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLockVerifyList>(data);
    }
}