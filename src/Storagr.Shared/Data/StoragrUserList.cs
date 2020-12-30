using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrUserListArgs : IStoragrListQuery
    {
        // [FromQuery(Name = "id")] public string Id;
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        [QueryMember(Name = "username")] public string Username { get; set; }

        // [FromQuery(Name = "refspec")] public string RefSpec;
    }
    
    [DataContract]
    public class StoragrUserList : IStoragrList<StoragrUser>
    {
        public static implicit operator StoragrUserList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrUserList>(data);
        
        [DataMember(Name = "users")] public IEnumerable<StoragrUser> Items { get; set; } = new List<StoragrUser>();
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; }

        public IEnumerator<StoragrUser> GetEnumerator() =>
            Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}