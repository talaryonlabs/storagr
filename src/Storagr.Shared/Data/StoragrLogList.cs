using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLogList : IStoragrList<StoragrLog, int>
    {
        public static implicit operator StoragrLogList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLogList>(data);
        
        [DataMember(Name = "logs")] public IEnumerable<StoragrLog> Items { get; set; } = new List<StoragrLog>();
        [DataMember(Name = "next_cursor")] public int NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; } 
        
        public IEnumerator<StoragrLog> GetEnumerator() =>
            Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
    
    [DataContract]
    public class StoragrLogQuery : IStoragrListQuery<int>
    {
        [QueryMember(Name = "cursor")] public int Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
    }
}