using Newtonsoft.Json;

namespace Storagr.Data
{
    [JsonObject]
    public class StoragrOwner
    {
        [JsonProperty("name")] public string Name { get; set; }
    }
}