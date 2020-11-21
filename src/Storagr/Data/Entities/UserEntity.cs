using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Shared.Data;

namespace Storagr.Data.Entities
{
    [Table("users")]
    public class UserEntity
    {
        [ExplicitKey] public string UserId { get; set; }
        public string AuthAdapter { get; set; }
        public string AuthId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Mail { get; set; }
        
        public static implicit operator UserModel([NotNull] UserEntity entity) => new UserModel()
        {
            UserId = entity.UserId,
            Username = entity.Username,
            Mail = entity.Mail,
            Role = entity.Role
        };
    }
}