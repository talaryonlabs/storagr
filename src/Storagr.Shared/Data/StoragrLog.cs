using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrLog
    {
        [JsonProperty("id")] public int LogId { get; set; }
        [JsonProperty("date")] public DateTime Date { get; set; }
        [JsonProperty("level")] public LogLevel Level { get; set; }
        [JsonProperty("category")] public string Category { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("exception")] public string Exception { get; set; }
    }
    
    [JsonObject]
    public class StoragrLogList : StoragrList<StoragrLog>
    {
        [JsonProperty("logs")] 
        public override IEnumerable<StoragrLog> Items { get; set; } = new List<StoragrLog>();
    }
    
    [DataContract]
    public class StoragrLogListArgs : StoragrListArgs
    {
        [QueryMember("category")] public string Category { get; set; }
        [QueryMember("level")] public LogLevel Level { get; set; }
        [QueryMember("message")] public string Message { get; set; }
    }
}