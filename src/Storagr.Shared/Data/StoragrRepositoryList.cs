using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrRepositoryListOptions
    {
        // [FromQuery(Name = "id")] public string Id;
        [QueryMember(Name = "cursor")] public string Cursor { get; set; }
        [QueryMember(Name = "limit")] public int Limit { get; set; }
        // [FromQuery(Name = "refspec")] public string RefSpec;
        
        public static StoragrRepositoryListOptions Empty => new StoragrRepositoryListOptions()
        {
            Cursor = null,
            Limit = 0
        };
    }
    
    [DataContract]
    public class StoragrRepositoryList : IEnumerable<StoragrRepository>
    {
        [DataMember(Name = "repositories")] public IEnumerable<StoragrRepository> Repositories;
        [DataMember(Name = "next_cursor")] public string NextCursor;
        public static StoragrRepositoryList Empty => new StoragrRepositoryList()
        {
            Repositories = new List<StoragrRepository>(),
            NextCursor = null
        };
        
        public static implicit operator StoragrRepositoryList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrRepositoryList>(data);

        public IEnumerator<StoragrRepository> GetEnumerator() =>
            Repositories.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}