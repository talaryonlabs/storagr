using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Services;
using Storagr.Shared.Data;

namespace Storagr.Data.Entities
{
    public enum RepositoryAccessType
    {
        Read,
        Write
    }
    
    [Table("Repository")]
    public class RepositoryEntity
    {
        [ExplicitKey] public string Id { get; set; }
        public string OwnerId { get; set; }
        public ulong SizeLimit { get; set; }

        public static implicit operator StoragrRepository([NotNull] RepositoryEntity entity) => new StoragrRepository()
        {
            RepositoryId = entity.Id,
            OwnerId = entity.OwnerId,
            SizeLimit = entity.SizeLimit
        };
        
        public static implicit operator RepositoryEntity([NotNull] StoragrRepository repository) => new RepositoryEntity()
        {
            Id = repository.RepositoryId,
            OwnerId = repository.OwnerId,
            SizeLimit = repository.SizeLimit
        };
    }
    
    [Table("repositories_access")]
    public class RepositoryAccessEntity
    {
        [ExplicitKey] public string RepositoryId { get; set; }
        [ExplicitKey] public string UserId { get; set; }
        
        public RepositoryAccessType AccessType { get; set; }
    }
}