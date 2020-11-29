using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Shared.Data;

namespace Storagr.Data.Entities
{
    [Table("Repository")]
    public class RepositoryEntity
    {
        [ExplicitKey] public string Id { get; set; }
        public string OwnerId { get; set; }
        
        public long SizeLimit { get; set; }

        [Computed] public UserEntity Owner { get; set; }
        [Computed] public IEnumerable<RepositoryAccessEntity> AccessEntities { get; set;}

        public static implicit operator StoragrRepository([NotNull] RepositoryEntity entity) => new StoragrRepository()
        {
            RepositoryId = entity.Id,
            OwnerId = entity.OwnerId,
            SizeLimit = entity.SizeLimit
        };
    }
    
    [Table("repositories_access")]
    public class RepositoryAccessEntity
    {
        [ExplicitKey] public string RepositoryId { get; set; }
        [ExplicitKey] public string UserId { get; set; }
        
        public int AccessType { get; set; }
    }
}