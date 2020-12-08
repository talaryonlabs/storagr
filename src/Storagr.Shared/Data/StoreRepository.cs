using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoreRepository
    {
        [DataMember(Name = "rid")] public string RepositoryId { get; set; }
        [DataMember(Name = "used_space")] public long UsedSpace { get; set; }
    }
}