﻿using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Shared.Data;

namespace Storagr.Server.Data.Entities
{
    [Table("User")]
    public class UserEntity
    {
        [ExplicitKey] public string Id { get; set; }
        public string AuthAdapter { get; set; }
        public string AuthId { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsAdmin { get; set; }
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        [Computed] public string Token { get; set; }
        
        public static implicit operator StoragrUser([NotNull] UserEntity entity) => new()
        {
            UserId = entity.Id,
            IsEnabled = entity.IsEnabled,
            IsAdmin = entity.IsAdmin,
            Username = entity.Username,
        };
        
        public static implicit operator UserEntity([NotNull] StoragrUser user) => new()
        {
            Id = user.UserId,
            Username = user.Username,
            IsAdmin = user.IsAdmin,
            IsEnabled = user.IsEnabled
        };
        
        public static implicit operator StoragrOwner([NotNull] UserEntity entity) => new()
        {
            Name = entity.Username,
        };
    }
}