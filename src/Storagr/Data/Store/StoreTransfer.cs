using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storagr.Data
{
    public enum StoreTransferType
    {
        Upload,
        Download
    }
    
    [JsonObject]
    public class StoreTransferRequest
    {
        [JsonConverter(typeof(StoreTransferType))]
        [JsonProperty("operation")] 
        public StoreTransferType Type { get; set; }
        
        [JsonProperty("repository")] public StoreRepository Repository { get; set; }
        [JsonProperty("objects")] public IEnumerable<string> Objects { get; set; }
    }

    [JsonObject]
    public class StoreTransferResponse
    {
        [JsonProperty("objects")] public IEnumerable<StoreTransferObject> Objects { get; set; } = new List<StoreTransferObject>();
    }

    [JsonObject]
    public class StoreTransferObject
    {
        [JsonProperty("tid")] public string TransferId { get; set; }
        [JsonProperty("oid")] public string ObjectId { get; set; }
        [JsonProperty("rid")] public string RepositoryId { get; set; }
    }
}