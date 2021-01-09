using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLockListArgs
    {
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        
        [QueryMember(Name = "path")] public string Path { get; set; }
        [QueryMember(Name = "id")] public string LockId { get; set; }
        
        [QueryMember(Name = "refspec")] public string RefSpec { get; set; }
    }
    
    [DataContract]
    public class StoragrLockList
    {
        [DataMember(Name = "locks")] public IEnumerable<StoragrLock> Items { get; set; } = new List<StoragrLock>();
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; }

        public static implicit operator StoragrLockList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLockList>(data);
    }
}