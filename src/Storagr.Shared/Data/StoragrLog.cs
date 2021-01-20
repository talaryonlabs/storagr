using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrLog
    {
        public static implicit operator StoragrLog(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLog>(data);
        
        [JsonProperty("id")] public int LogId { get; set; }
        [JsonProperty("date")] public DateTime Date { get; set; }
        [JsonProperty("level")] public LogLevel Level { get; set; }
        [JsonProperty("category")] public string Category { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("exception")] public string Exception { get; set; }
    }
}