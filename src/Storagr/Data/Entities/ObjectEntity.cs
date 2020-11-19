using System.Runtime.Serialization;
using Dapper.Contrib.Extensions;

namespace Storagr.Data.Entities
{
    [Table("objects")]
    public class ObjectEntity
    {
        [ExplicitKey] public string ObjectId { get; set; }
        [ExplicitKey] public string RepositoryId { get; set; }
        public long Size { get; set; }
    }
}