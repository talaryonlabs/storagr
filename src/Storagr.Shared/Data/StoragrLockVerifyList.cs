using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLockVerifyList
    {
        public static implicit operator StoragrLockVerifyList(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrLockVerifyList>(data);
        
        [DataMember(Name = "ours")] public IEnumerable<StoragrLock> Ours { get; set; } = new List<StoragrLock>();
        [DataMember(Name = "theirs")] public IEnumerable<StoragrLock> Theirs { get; set; } = new List<StoragrLock>();
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
        [DataMember(Name = "total_count")] public int TotalCount { get; set; }
    }
    
    [DataContract]
    public class StoragrLockVerifyListArgs : IStoragrListQuery
    {
        [DataMember(Name = "cursor")] public string Cursor { get; set; }
        [DataMember(Name = "limit")] public int Limit { get; set; }
        [DataMember(Name = "ref")] public StoragrRef Ref { get; set; }
    }
}