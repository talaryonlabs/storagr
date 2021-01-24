using Newtonsoft.Json;

namespace Storagr.Data
{
    [JsonObject]
    public class StoreRepository
    {
        [JsonProperty("rid")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("size")] public ulong Size { get; set; }
        
    }
}