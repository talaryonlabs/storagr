using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Shared.Data;

namespace Storagr.Data.Entities
{
    [Table("repositories")]
    public class RepositoryEntity
    {
        [ExplicitKey] public string RepositoryId { get; set; }
        public string OwnerId { get; set; }
        
        public long SizeLimit { get; set; }

        [Computed] public UserEntity Owner { get; set; }

        public static implicit operator StoragrRepository([NotNull] RepositoryEntity entity) => new StoragrRepository()
        {
            RepositoryId = entity.RepositoryId,
            OwnerId = entity.OwnerId
        };
    }
}