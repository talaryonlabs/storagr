using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storagr.Data
{
    public enum StoragrBatchOperation
    {
        Download,
        Upload,
    }
    
    [JsonObject]
    public class StoragrBatchError
    {
        [JsonProperty("code")] public int Code;
        [JsonProperty("message")] public string Message;

        public static implicit operator StoragrBatchError(StoragrError error) => new()
        {
            Code = error.Code,
            Message = error.Message
        };
    }
    
    [JsonObject]
    public class StoragrBatchRequest
    {
        [JsonConverter(typeof(StoragrBatchOperation))]
        [JsonProperty("operation", Required = Required.Always)]  
        public StoragrBatchOperation Operation  { get; set; }
        
        [JsonProperty("transfers")] public IEnumerable<string> Transfers  { get; set; } = new[] {"basic"};
        [JsonProperty("ref")] public StoragrRef Ref { get; set; }
        [JsonProperty("objects", Required = Required.Always)] public IEnumerable<StoragrObject> Objects { get; set; }
    }

    [JsonObject]
    public class StoragrBatchResponse
    {
        [JsonProperty("transfer")] public string TransferAdapter { get; set; }
        [JsonProperty("objects")] public IEnumerable<StoragrBatchObject> Objects { get; set; }
    }
    
    [JsonObject]
    public class StoragrBatchObject : StoragrObject
    {
        [JsonProperty("authenticated")] public bool Authenticated { get; set; }
        [JsonProperty("actions")] public StoragrActions Actions { get; set; }
        [JsonProperty("error")] public StoragrBatchError Error { get; set; }
    }
}