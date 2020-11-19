using Dapper.Contrib.Extensions;

namespace Storagr.Data.Entities
{
    [Table("repositories")]
    public class RepositoryEntity
    {
        [ExplicitKey] public string RepositoryId { get; set; }
        public string OwnerId { get; set; }
    }
}