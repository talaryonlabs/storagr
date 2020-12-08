using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLockVerifyListRequest
    {
        [DataMember(Name = "cursor")] public string Cursor { get; set; }
        [DataMember(Name = "limit")] public int Limit { get; set; }
        [DataMember(Name = "ref")] public StoragrRef Ref { get; set; }
    }

    [DataContract]
    public class StoragrLockVerifyList
    {
        [DataMember(Name = "ours")] public List<StoragrLock> Ours { get; set; }
        [DataMember(Name = "theirs")] public List<StoragrLock> Theirs { get; set; }
        [DataMember(Name = "next_cursor")] public string NextCursor { get; set; }
    }
}