using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrObjectListOptions
    {
        // [FromQuery(Name = "id")] public string Id;
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        // [FromQuery(Name = "refspec")] public string RefSpec;
        
        public static StoragrObjectListOptions Empty => new StoragrObjectListOptions()
        {
            Cursor = null,
            Limit = 0
        };
    }
    
    [DataContract]
    public class StoragrObjectList
    {
        [DataMember(Name = "objects")] public IEnumerable<StoragrObject> Objects;
        [DataMember(Name = "next_cursor")] public string NextCursor;
        public static StoragrObjectList Empty => new StoragrObjectList()
        {
            Objects = new List<StoragrObject>(),
            NextCursor = null
        };
        
        public static implicit operator StoragrObjectList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrObjectList>(data);
    }
}