using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrAuthenticationRequest
    {
        [JsonProperty("username", Required = Required.Always)] public string Username { get; set; }
        [JsonProperty("password", Required = Required.Always)] public string Password { get; set; }
    }

    [JsonObject]
    public class StoragrAuthenticationResponse
    {
        [JsonProperty("token")] public string Token { get; set; }
    }
}