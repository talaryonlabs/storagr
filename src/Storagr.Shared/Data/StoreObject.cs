using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoreObject
    {
        [DataMember(Name = "oid")] public string ObjectId { get; set; }
        [DataMember(Name = "rid")] public string RepositoryId { get; set; }
        [DataMember(Name = "size")] public long Size { get; set; }
    }
}