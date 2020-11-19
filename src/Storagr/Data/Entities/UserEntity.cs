using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using Storagr.Controllers.Models;

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
    }
}