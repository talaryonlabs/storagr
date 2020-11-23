using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrUser
    {
        [DataMember(Name = "uid")] public string UserId;
        [DataMember(Name = "enabled")] public bool IsEnabled;
        [DataMember(Name = "username")] public string Username;
        [DataMember(Name = "mail")] public string Mail;
        [DataMember(Name = "role")] public string Role;
    }
    
    [DataContract]
    public class StoragrUserRequest
    {
        [DataMember(Name = "username", IsRequired = true)] public string Username;
        [DataMember(Name = "password", IsRequired = true)] public string Password;
        [DataMember(Name = "mail")] public string Mail;
        [DataMember(Name = "role")] public string Role;
    }
}