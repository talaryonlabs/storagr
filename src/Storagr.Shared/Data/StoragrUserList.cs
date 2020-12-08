using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrUserListOptions
    {
        // [FromQuery(Name = "id")] public string Id;
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        // [FromQuery(Name = "refspec")] public string RefSpec;
        
        public static StoragrUserListOptions Empty => new StoragrUserListOptions()
        {
            Cursor = null,
            Limit = 0
        };
    }
    
    [DataContract]
    public class StoragrUserList : IEnumerable<StoragrUser>
    {
        [DataMember(Name = "users")] public IEnumerable<StoragrUser> Users;
        [DataMember(Name = "next_cursor")] public string NextCursor;
        public static StoragrUserList Empty => new StoragrUserList()
        {
            Users = new List<StoragrUser>(),
            NextCursor = null
        };
        
        public static implicit operator StoragrUserList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrUserList>(data);

        public IEnumerator<StoragrUser> GetEnumerator() =>
            Users.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}