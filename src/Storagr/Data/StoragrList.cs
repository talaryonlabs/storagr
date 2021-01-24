using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Storagr.Data
{
    [JsonObject]
    public abstract class StoragrList<T> : IStoragrList<T>
    {
        public abstract IEnumerable<T> Items { get; set; }
        
        [JsonProperty("next_cursor")] public string NextCursor { get; set; }
        [JsonProperty("total_count")] public int TotalCount { get; set; }
    }
    
    [DataContract]
    public abstract class StoragrListArgs : IStoragrListArgs
    {
        [QueryMember("cursor")] public string Cursor { get; set; }
        [QueryMember("skip")] public int Skip { get; set; }
        [QueryMember("limit")] public int Limit { get; set; }
    }
}