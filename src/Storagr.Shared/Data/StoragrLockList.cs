﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLockListRequest
    {
        [FromQuery(Name = "path")] public string Path { get; set; }
        [FromQuery(Name = "id")] public string LockId { get; set; }
        [FromQuery(Name = "cursor")] public string Cursor { get; set; }
        [FromQuery(Name = "limit")] public int Limit { get; set; }
        [FromQuery(Name = "refspec")] public string RefSpec { get; set; }
    }

    [DataContract]
    public class StoragrLockListResponse
    {
        [DataMember(Name = "locks")] public IEnumerable<StoragrLock> Locks;
        [DataMember(Name = "next_cursor")] public string NextCursor;

        public static StoragrLockListResponse Empty => new StoragrLockListResponse()
        {
            Locks = new StoragrLock[0],
            NextCursor = null
        };
    }
}