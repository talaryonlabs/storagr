using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrAuthenticationRequest
    {
        [DataMember(Name = "username", IsRequired = true)] public string Username { get; set; }
        [DataMember(Name = "password", IsRequired = true)] public string Password { get; set; }
    }

    [DataContract]
    public class StoragrAuthenticationResponse
    {
        [DataMember(Name = "token")] public string Token { get; set; }
    }
}