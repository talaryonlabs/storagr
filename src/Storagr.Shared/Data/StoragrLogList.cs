using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{    
    [DataContract]
    public class StoragrLogListArgs : IStoragrListArgs
    {
        [QueryMember("cursor")] public string Cursor { get; set; }
        [QueryMember("skip")] public int Skip { get; set; }
        [QueryMember("limit")] public int Limit { get; set; }
        
        [QueryMember("category")] public string Category { get; set; }
        [QueryMember("level")] public LogLevel Level { get; set; }
        [QueryMember("message")] public string Message { get; set; }
    }
    
    [JsonObject]
    public class StoragrLogList : IStoragrList<StoragrLog>
    {
        [JsonProperty("logs")] public IEnumerable<StoragrLog> Items { get; set; } = new List<StoragrLog>();
        [JsonProperty("next_cursor")] public string NextCursor { get; set; }
        [JsonProperty("total_count")] public int TotalCount { get; set; }
        
        public static implicit operator StoragrLogList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLogList>(data);
    }
}