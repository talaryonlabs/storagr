using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

namespace Storagr.Shared.Data
{    
    [DataContract]
    public class StoragrLogListArgs : IStoragrListArgs
    {
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "skip")] public int Skip { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        
        [QueryMember(Name = "category")] public string Category { get; set; }
        [QueryMember(Name = "level")] public LogLevel Level { get; set; }
        [QueryMember(Name = "message")] public string Message { get; set; }
    }
    
    [DataContract]
    public class StoragrLogList : IStoragrList<StoragrLog>
    {
        [DataMember(Name = "logs")] public IEnumerable<StoragrLog> Items { get; set; } = new List<StoragrLog>();
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; }
        
        public static implicit operator StoragrLogList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLogList>(data);
    }
}