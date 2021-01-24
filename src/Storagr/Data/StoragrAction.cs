using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storagr.Data
{
    [JsonObject]
    public class StoragrAction
    {
        [JsonProperty("href", Required = Required.Always)] public string Href { get; set; }
        [JsonProperty("header")] public IDictionary<string, string> Header { get; set; }
        [JsonProperty("expires_in")] public int ExpiresIn { get; set; }
        [JsonProperty("expires_at")] public DateTime ExpiresAt { get; set; }
    }

    [JsonObject]
    public class StoragrActions
    {
        [JsonProperty("download")] public StoragrAction Download { get; set; }
        [JsonProperty("upload")] public StoragrAction Upload { get; set; }
        [JsonProperty("verify")] public StoragrAction Verify { get; set; }
    }
}