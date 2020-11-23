﻿using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoreRepository
    {
        [DataMember(Name = "rid")] public string RepositoryId;
        [DataMember(Name = "used_space")] public long UsedSpace;
    }
}