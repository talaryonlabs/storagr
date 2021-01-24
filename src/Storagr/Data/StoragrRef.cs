using Newtonsoft.Json;

namespace Storagr.Data
{
    [JsonObject]
    public class StoragrRef
    {
        [JsonProperty("name")] public string Name { get; set; }
    }
}