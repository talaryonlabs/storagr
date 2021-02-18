using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoreObject
    {
        [JsonProperty("oid")] public string ObjectId { get; set; }
        [JsonProperty("size")] public long Size { get; set; }
    }
}