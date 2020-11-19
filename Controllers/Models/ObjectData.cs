using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Storagr.Data.Entities;

namespace Storagr.Controllers.Models
{
    [DataContract]
    public class ObjectModel
    {
        [DataMember(Name = "oid", IsRequired = true)] public string ObjectId;
        [DataMember(Name = "repositoryId", IsRequired = true)] public string RepositoryId;
        [DataMember(Name = "size", IsRequired = true)] public long Size;
        
        public static implicit operator ObjectModel([NotNull] ObjectEntity entity) => new ObjectModel()
        {
            ObjectId = entity.ObjectId,
            RepositoryId = entity.RepositoryId,
            Size =  entity.Size
        };
    }
    
    [DataContract]
    public class ObjectListRequest
    {
        // [FromQuery(Name = "id")] public string Id { get; set; }
        [FromQuery(Name = "cursor")] public string Cursor { get; set; }
        [FromQuery(Name = "limit")] public int Limit { get; set; }
        // [FromQuery(Name = "refspec")] public string RefSpec { get; set; }
    }
    
    [DataContract]
    public class ObjectListResponse
    {
        [DataMember(Name = "objects")] public IEnumerable<ObjectModel> Objects;
        [DataMember(Name = "next_cursor")] public string NextCursor;
    }
    
    
    
    
    
    
    
}