﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoreObject
    {
        [DataMember(Name = "oid")] public string ObjectId;
        [DataMember(Name = "rid")] public string RepositoryId;
        [DataMember(Name = "size")] public long Size;
    }
}