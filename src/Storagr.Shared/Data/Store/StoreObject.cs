using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoreObject
    {
        [JsonProperty("oid")] public string ObjectId { get; set; }
        [JsonProperty("rid")] public string RepositoryId { get; set; }
        [JsonProperty("size")] public ulong Size { get; set; }
    }
}