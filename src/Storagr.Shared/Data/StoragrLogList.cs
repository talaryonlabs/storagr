using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLogListOptions
    {
        [QueryMember(Name = "offset")] public int Offset { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        
        public static StoragrLogListOptions Empty => new StoragrLogListOptions()
        {
            Offset = 0,
            Limit = 0
        };
    }
    
    [DataContract]
    public class StoragrLogList
    {
        [DataMember(Name = "logs")] public IEnumerable<StoragrLog> Logs { get; set; }
        [DataMember(Name = "cursor")] public int Cursor { get; set; }
        [DataMember(Name = "total")] public int Total { get; set; }
        public static StoragrLogList Empty => new StoragrLogList()
        {
            Logs = new List<StoragrLog>(),
            Cursor = -1,
            Total = 0
        };
    }
}