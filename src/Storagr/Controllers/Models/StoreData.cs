using System.Runtime.Serialization;

namespace Storagr.Controllers.Models
{
    [DataContract]
    public class StoreVerifyRequest
    {
        [DataMember(Name = "oid")] public string ObjectId;
        [DataMember(Name = "size")] public long Size;
    }
}