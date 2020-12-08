using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrObjectListQuery : IStoragrListQuery
    {
        // [FromQuery(Name = "id")] public string Id;
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        // [FromQuery(Name = "refspec")] public string RefSpec;
    }
    
    [DataContract]
    public class StoragrObjectList : IStoragrList<StoragrObject>
    {
        [DataMember(Name = "objects")] public IEnumerable<StoragrObject> Items { get; set; } = new List<StoragrObject>();
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; }

        public IEnumerator<StoragrObject> GetEnumerator() =>
            Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
        
        public static implicit operator StoragrObjectList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrObjectList>(data);
    }
}