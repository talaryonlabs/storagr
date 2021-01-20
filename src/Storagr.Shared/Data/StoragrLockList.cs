using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLockListArgs : IStoragrListArgs
    {
        [QueryMember("cursor")] public string Cursor { get; set; }
        [QueryMember("limit")] public int Limit { get; set; }
        [QueryMember("skip")] public int Skip { get; set; }
        
        [QueryMember("path")] public string Path { get; set; }
        [QueryMember("id")] public string Id { get; set; }
        
        [QueryMember("refspec")] public string RefSpec { get; set; }
    }
    
    [JsonObject]
    public class StoragrLockList : IStoragrList<StoragrLock>
    {
        [JsonProperty("locks")] public IEnumerable<StoragrLock> Items { get; set; } = new List<StoragrLock>();
        [JsonProperty("next_cursor")] public string NextCursor { get; set; }
        [JsonProperty("total_count")] public int TotalCount { get; set; }

        public static implicit operator StoragrLockList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLockList>(data);
    }
}