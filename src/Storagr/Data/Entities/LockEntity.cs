using System;
using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Services;
using Storagr.Shared.Data;

namespace Storagr.Data.Entities
{
    [Table("Lock")]
    public class LockEntity
    {
        [ExplicitKey] public string Id { get; set; }
        [ExplicitKey] public string RepositoryId { get; set; }
        public string OwnerId { get; set; }
        public string Path { get; set; }
        public DateTime LockedAt { get; set; }

        [Computed] public UserEntity Owner { get; set; }
        
        public static implicit operator StoragrLock([NotNull] LockEntity entity) => new ()
        {
            LockId = entity.Id,
            Path = entity.Path,
            LockedAt = entity.LockedAt,
            Owner = entity.Owner
        };
    }
}