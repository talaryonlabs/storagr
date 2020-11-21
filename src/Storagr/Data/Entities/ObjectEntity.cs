using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Dapper.Contrib.Extensions;
using Storagr.Shared.Data;

namespace Storagr.Data.Entities
{
    [Table("objects")]
    public class ObjectEntity
    {
        [ExplicitKey] public string ObjectId { get; set; }
        [ExplicitKey] public string RepositoryId { get; set; }
        public long Size { get; set; }
        
        public static implicit operator ObjectModel([NotNull] ObjectEntity entity) => new ObjectModel()
        {
            ObjectId = entity.ObjectId,
            RepositoryId = entity.RepositoryId,
            Size =  entity.Size
        };
    }
}