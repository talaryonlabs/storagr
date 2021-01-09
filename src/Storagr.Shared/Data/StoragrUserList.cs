using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrUserListArgs
    {
        // [FromQuery(Name = "id")] public string Id;
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        [QueryMember(Name = "username")] public string Username { get; set; }

        // [FromQuery(Name = "refspec")] public string RefSpec;
    }
    
    [DataContract]
    public class StoragrUserList
    {
        [DataMember(Name = "users")] public IEnumerable<StoragrUser> Items { get; set; } = new List<StoragrUser>();
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; }
        
        public static implicit operator StoragrUserList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrUserList>(data);
    }
}