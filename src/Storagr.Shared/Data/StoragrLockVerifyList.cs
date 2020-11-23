using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLockVerifyListRequest
    {
        [DataMember(Name = "cursor")] public string Cursor;
        [DataMember(Name = "limit")] public int Limit;
        [DataMember(Name = "ref")] public StoragrRef Ref;
    }

    [DataContract]
    public class StoragrLockVerifyListResponse
    {
        [DataMember(Name = "ours")] public List<StoragrLock> Ours;
        [DataMember(Name = "theirs")] public List<StoragrLock> Theirs;
        [DataMember(Name = "next_cursor")] public string NextCursor;
    }
}