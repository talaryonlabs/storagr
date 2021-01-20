using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrObjectListArgs : IStoragrListArgs
    {
        
        [QueryMember("cursor")] public string Cursor { get; set; }
        [QueryMember("limit")] public int Limit { get; set; }
        [QueryMember("skip")] public int Skip { get; set; }
        
        [QueryMember("id")] public string Id { get; set; }
        // [QueryMember(Name = "refspec")] public string RefSpec;
    }
    
    [JsonObject]
    public class StoragrObjectList : IStoragrList<StoragrObject>
    {
        [JsonProperty("objects")] public IEnumerable<StoragrObject> Items { get; set; } = new List<StoragrObject>();
        [JsonProperty("next_cursor")] public string NextCursor { get; set; }
        [JsonProperty("total_count")] public int TotalCount { get; set; }
        
        public static implicit operator StoragrObjectList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrObjectList>(data);
    }
}