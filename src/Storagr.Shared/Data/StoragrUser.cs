using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrUser
    {
        [JsonProperty("uid")] public string UserId { get; set; }
        [JsonProperty("username")] public string Username { get; set; }
        [JsonProperty("is_enabled")] public bool IsEnabled { get; set; }
        [JsonProperty("is_admin")] public bool IsAdmin { get; set; }
        
        public static implicit operator StoragrUser(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrUser>(data);
    }
}