using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrRepositoryListArgs : IStoragrListQuery
    {
        [QueryMember(Name = "id")] public string Id { get; set; }
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        // [FromQuery(Name = "refspec")] public string RefSpec;
    }
    
    [DataContract]
    public class StoragrRepositoryList : IStoragrList<StoragrRepository>
    {
        public static implicit operator StoragrRepositoryList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrRepositoryList>(data);
        
        [DataMember(Name = "repositories")] public IEnumerable<StoragrRepository> Items { get; set; } = new List<StoragrRepository>();
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; }

        public IEnumerator<StoragrRepository> GetEnumerator() =>
            Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}