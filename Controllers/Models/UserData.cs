using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Storagr.Data.Entities;

namespace Storagr.Controllers.Models
{
    [DataContract]
    public class UserModel
    {
        [DataMember(Name = "uid")] public string UserId;
        [DataMember(Name = "username")] public string Username;
        [DataMember(Name = "mail")] public string Mail;
        [DataMember(Name = "role")] public string Role;

        public static implicit operator UserModel([NotNull] UserEntity entity) => new UserModel()
        {
            UserId = entity.UserId,
            Username = entity.Username,
            Mail = entity.Mail,
            Role = entity.Role
        };
    }
    
    [DataContract]
    public class UserLoginRequest
    {
        [DataMember(Name = "username", IsRequired = true)] public string Username;
        [DataMember(Name = "password", IsRequired = true)] public string Password;
    }
    
    [DataContract]
    public class UserLoginResponse
    {
        [DataMember(Name = "token")] public string Token;
    }

    [DataContract]
    public class UserListResponse
    {
        [DataMember(Name = "users")] public IEnumerable<UserModel> Users;
    }

    [DataContract]
    public class UserCreateRequest
    {
        [DataMember(Name = "username", IsRequired = true)] public string Username;
        [DataMember(Name = "password", IsRequired = true)] public string Password;
        [DataMember(Name = "mail")] public string Mail;
        [DataMember(Name = "role")] public string Role;
    }

    [DataContract]
    public class UserModifyRequest
    {
        [DataMember(Name = "username")] public string Username;
        [DataMember(Name = "password")] public string Password;
        [DataMember(Name = "mail")] public string Mail;
        [DataMember(Name = "role")] public string Role;
    }
}