using System;
using Dapper.Contrib.Extensions;

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
    }
}