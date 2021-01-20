using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrRef
    {
        [JsonProperty("name")] public string Name { get; set; }
    }
}