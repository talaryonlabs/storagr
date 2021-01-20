using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrRepository
    {
        [JsonProperty("rid", Required = Required.Always)] public string RepositoryId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("owner")] public string Owner { get; set; }
        [JsonProperty("size_limit")] public ulong SizeLimit { get; set; }
    }
    
    [JsonObject]
    public class StoragrRepositoryList : StoragrList<StoragrRepository>
    {
        [JsonProperty("repositories")]
        public override IEnumerable<StoragrRepository> Items { get; set; } = new List<StoragrRepository>();
    }
    
    [DataContract]
    public class StoragrRepositoryListArgs : StoragrListArgs
    {
        [QueryMember("id")] public string Id { get; set; }
        [QueryMember("name")] public string Name { get; set; }
        [QueryMember("owner")] public string Owner { get; set; }
        [QueryMember("size_limit")] public ulong SizeLimit { get; set; }

        // [QueryMember("refspec")] public string RefSpec;
    }
}