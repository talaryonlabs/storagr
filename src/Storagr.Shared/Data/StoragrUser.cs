using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrUser
    {
        [DataMember(Name = "uid")] public string UserId { get; set; }
        [DataMember(Name = "is_enabled")] public bool IsEnabled { get; set; }
        [DataMember(Name = "is_admin")] public bool IsAdmin { get; set; }
        [DataMember(Name = "username")] public string Username { get; set; }
        
        public static implicit operator StoragrUser(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrUser>(data);
    }
    
    [DataContract]
    public class StoragrUserRequest
    {
        [DataMember(Name = "user", IsRequired = true)] public StoragrUser User { get; set; }
        [DataMember(Name = "new_password")] public string NewPassword { get; set; }
    }
}