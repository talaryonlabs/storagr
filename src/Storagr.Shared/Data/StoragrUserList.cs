using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrUserListArgs : IStoragrListArgs
    {
        [QueryMember("cursor")] public string Cursor { get; set; }
        [QueryMember("limit")] public int Limit { get; set; }
        [QueryMember("skip")] public int Skip { get; set; }
        
        [QueryMember("id")] public string Id { get; set; }
        [QueryMember("username")] public string Username { get; set; }
        [QueryMember("is_enabled")] public bool IsEnabled { get; set; }
        [QueryMember("is_admin")] public bool IsAdmin { get; set; }
        // [QueryMember("refspec")] public string RefSpec;
    }
    
    [JsonObject]
    public class StoragrUserList : IStoragrList<StoragrUser>
    {
        [JsonProperty("users")] public IEnumerable<StoragrUser> Items { get; set; } = new List<StoragrUser>();
        [JsonProperty("next_cursor")] public string NextCursor { get; set; }
        [JsonProperty("total_count")] public int TotalCount { get; set; }
        
        public static implicit operator StoragrUserList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrUserList>(data);
    }
}