using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Shared.Data;

namespace Storagr.Server.Data.Entities
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
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public ulong SizeLimit { get; set; }
        
        [Computed] public string OwnerName { get; set; }

        public static implicit operator StoragrRepository([NotNull] RepositoryEntity entity) => new()
        {
            RepositoryId = entity.Id,
            Name = entity.Name,
            Owner = entity.OwnerId,
            SizeLimit = entity.SizeLimit
        };
        
        public static implicit operator RepositoryEntity([NotNull] StoragrRepository repository) => new()
        {
            Id = repository.RepositoryId,
            Name = repository.Name,
            OwnerId = repository.Owner,
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