using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLockList : IStoragrList<StoragrLock>
    {
        public static implicit operator StoragrLockList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLockList>(data);
        
        [DataMember(Name = "locks")] public IEnumerable<StoragrLock> Items { get; set; } = new List<StoragrLock>();
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; }

        
        public IEnumerator<StoragrLock> GetEnumerator() =>
            Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
    
    [DataContract]
    public class StoragrLockListArgs : IStoragrListQuery
    {
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        
        [QueryMember(Name = "path")] public string Path { get; set; }
        [QueryMember(Name = "id")] public string LockId { get; set; }
        
        [QueryMember(Name = "refspec")] public string RefSpec { get; set; }
    }
}