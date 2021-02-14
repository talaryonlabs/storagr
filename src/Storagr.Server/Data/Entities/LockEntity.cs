using System;
using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Shared.Data;

namespace Storagr.Server.Data.Entities
{
    [Table("Lock")]
    public class LockEntity
    {
        [ExplicitKey] public string Id { get; set; }
        [ExplicitKey] public string RepositoryId { get; set; }
        public string OwnerId { get; set; }
        public string Path { get; set; }
        public DateTime LockedAt { get; set; }
        
        [Computed] public string RepositoryName { get; set; }
        [Computed] public string OwnerName { get; set; }
        
        public static implicit operator StoragrLock([NotNull] LockEntity entity) => new()
        {
            LockId = entity.Id,
            LockedAt = entity.LockedAt,
            Path = entity.Path,
            Owner = new StoragrOwner()
            {
                Id = entity.Id,
                Name = entity.OwnerName
            }
        };
    }
}