using System;
using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Client.Models;

namespace Storagr.Data.Entities
{
    [Table("locks")]
    public class LockEntity
    {
        [ExplicitKey] public string LockId { get; set; }
        [ExplicitKey] public string RepositoryId { get; set; }
        public string OwnerId { get; set; }
        public string Path { get; set; }
        public DateTime LockedAt { get; set; }

        [Computed] public UserEntity Owner { get; set; }
        [Computed] public RepositoryEntity Repository { get; set; }
        
        public static implicit operator LockModel([NotNull] LockEntity entity) => new LockModel()
        {
            LockId = entity.LockId,
            Path = entity.Path,
            LockedAt = entity.LockedAt,
            Owner = new OwnerData()
            {
                Name = entity.Owner.Username
            }
        };
    }
}