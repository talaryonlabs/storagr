using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrRepository
    {
        [DataMember(Name = "rid")] public string RepositoryId;
        [DataMember(Name = "owner_uid")] public string OwnerId;
    }
}