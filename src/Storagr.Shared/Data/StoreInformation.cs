using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoreInformation
    {
        [JsonProperty("available_space")] public long AvailableSpace { get; set; }
    }
}