using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrOwner
    {
        [JsonProperty("name")] public string Name { get; set; }
    }
}