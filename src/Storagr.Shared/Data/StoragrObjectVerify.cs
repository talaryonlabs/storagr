using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrObjectVerifyRequest
    {
        [DataMember(Name = "oid")] public string ObjectId;
        [DataMember(Name = "size")] public long Size;
    }
}