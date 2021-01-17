using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrUserListArgs : IStoragrListArgs
    {
        
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        [QueryMember(Name = "skip")] public int Skip { get; set; }
        
        [QueryMember(Name = "id")] public string Id { get; set; }
        [QueryMember(Name = "username")] public string Username { get; set; }
        [QueryMember(Name = "is_enabled")] public bool IsEnabled { get; set; }
        [QueryMember(Name = "is_admin")] public bool IsAdmin { get; set; }
        // [FromQuery(Name = "refspec")] public string RefSpec;
    }
    
    [DataContract]
    public class StoragrUserList : IStoragrList<StoragrUser>
    {
        [DataMember(Name = "users")] public IEnumerable<StoragrUser> Items { get; set; } = new List<StoragrUser>();
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; }
        
        public static implicit operator StoragrUserList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrUserList>(data);
    }
}