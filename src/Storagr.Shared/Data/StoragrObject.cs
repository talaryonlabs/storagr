using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrObject
    {
        [JsonProperty("oid")] public string ObjectId { get; set; }
        [JsonProperty("rid")] public string RepositoryId { get; set; }
        [JsonProperty("size")] public ulong Size { get; set; }
    }
    
    [JsonObject]
    public class StoragrObjectList : StoragrList<StoragrObject>
    {
        [JsonProperty("objects")] 
        public override IEnumerable<StoragrObject> Items { get; set; } = new List<StoragrObject>();
    }
    
    [DataContract]
    public class StoragrObjectListArgs : StoragrListArgs
    {
        [QueryMember("id")] public string Id { get; set; }
        // [QueryMember(Name = "refspec")] public string RefSpec;
    }
}