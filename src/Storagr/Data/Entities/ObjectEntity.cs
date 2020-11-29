﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Dapper.Contrib.Extensions;
using Storagr.Shared.Data;

namespace Storagr.Data.Entities
{
    [Table("Object")]
    public class ObjectEntity
    {
        [ExplicitKey] public string Id { get; set; }
        [ExplicitKey] public string RepositoryId { get; set; }
        public long Size { get; set; }
        
        public static implicit operator StoragrObject([NotNull] ObjectEntity entity) => new StoragrObject()
        {
            ObjectId = entity.Id,
            Size =  entity.Size
        };
    }
}