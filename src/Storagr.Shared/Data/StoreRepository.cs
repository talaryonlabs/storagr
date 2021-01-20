using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoreRepository
    {
        [JsonProperty("rid")] public string RepositoryId { get; set; }
        [JsonProperty("used_space")] public long UsedSpace { get; set; }
    }
}