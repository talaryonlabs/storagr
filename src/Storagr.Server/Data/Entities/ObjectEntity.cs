using System.Diagnostics.CodeAnalysis;
using Dapper.Contrib.Extensions;
using Storagr.Data;

namespace Storagr.Server.Data.Entities
{
    [Table("Object")]
    public class ObjectEntity
    {
        [ExplicitKey] public string Id { get; set; }
        [ExplicitKey] public string RepositoryId { get; set; }
        public ulong Size { get; set; }
        
        public static implicit operator StoragrObject([NotNull] ObjectEntity entity) => new ()
        {
            ObjectId = entity.Id,
            RepositoryId = entity.RepositoryId,
            Size =  entity.Size
        };
        
        public static implicit operator ObjectEntity([NotNull] StoragrObject obj) => new ()
        {
            Id = obj.ObjectId,
            RepositoryId = obj.RepositoryId,
            Size =  obj.Size
        };
    }
}