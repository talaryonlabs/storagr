using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLockListOptions
    {
        [QueryMember(Name = "path")] public string Path { get; set; }
        [QueryMember(Name = "id")] public string LockId { get; set; }
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        [QueryMember(Name = "refspec")] public string RefSpec { get; set; }

        public static StoragrLockListOptions Empty => new StoragrLockListOptions()
        {
            LockId = null,
            Path = null,
            Cursor = null,
            Limit = 0,
            RefSpec = null
        };
    }

    [DataContract]
    public class StoragrLockList
    {
        [DataMember(Name = "locks")] public IEnumerable<StoragrLock> Locks;
        [DataMember(Name = "next_cursor")] public string NextCursor;

        public static StoragrLockList Empty => new StoragrLockList()
        {
            Locks = new StoragrLock[0],
            NextCursor = null
        };
        
        public static implicit operator StoragrLockList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLockList>(data);
    }
}