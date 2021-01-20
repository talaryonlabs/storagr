using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrObject
    {
        [JsonProperty("oid")] public string ObjectId { get; set; }
        [JsonProperty("rid")] public string RepositoryId { get; set; }
        [JsonProperty("size")] public long Size { get; set; }

        public static implicit operator StoragrObject(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrObject>(data);
    }
}