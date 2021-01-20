using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrUser
    {
        [JsonProperty("uid")] public string UserId { get; set; }
        [JsonProperty("username")] public string Username { get; set; }
        [JsonProperty("is_enabled")] public bool IsEnabled { get; set; }
        [JsonProperty("is_admin")] public bool IsAdmin { get; set; }
    }
    
    [JsonObject]
    public class StoragrUserList : StoragrList<StoragrUser>
    {
        [JsonProperty("users")] 
        public override IEnumerable<StoragrUser> Items { get; set; } = new List<StoragrUser>();
    }
    
    [DataContract]
    public class StoragrUserListArgs : StoragrListArgs
    {
        [QueryMember("id")] public string Id { get; set; }
        [QueryMember("username")] public string Username { get; set; }
        [QueryMember("is_enabled")] public bool IsEnabled { get; set; }
        [QueryMember("is_admin")] public bool IsAdmin { get; set; }
        // [QueryMember("refspec")] public string RefSpec;
    }
}