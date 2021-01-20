using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrRepositoryListArgs : IStoragrListArgs
    {
        [QueryMember("cursor")] public string Cursor { get; set; }
        [QueryMember("limit")] public int Limit { get; set; }
        [QueryMember("skip")] public int Skip { get; set; }

        [QueryMember("id")] public string Id { get; set; }
        [QueryMember("name")] public string Name { get; set; }
        [QueryMember("owner")] public string Owner { get; set; }
        [QueryMember("size_limit")] public ulong SizeLimit { get; set; }

        // [QueryMember("refspec")] public string RefSpec;
    }
    
    [JsonObject]
    public class StoragrRepositoryList : IStoragrList<StoragrRepository>
    {
        [JsonProperty("repositories")] public IEnumerable<StoragrRepository> Items { get; set; } = new List<StoragrRepository>();
        [JsonProperty("next_cursor")] public string NextCursor { get; set; }
        [JsonProperty("total_count")] public int TotalCount { get; set; }
        
        public static implicit operator StoragrRepositoryList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrRepositoryList>(data);
    }
}