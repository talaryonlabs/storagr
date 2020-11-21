using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoreRepository
    {
        [DataMember(Name = "rid")] public string RepositoryId { get; set; }
        [DataMember(Name = "usedSpace")] public long UsedSpace { get; set; }
    }
}