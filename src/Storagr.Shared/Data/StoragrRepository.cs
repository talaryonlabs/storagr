using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrRepository
    {
        [DataMember(Name = "rid", IsRequired = true)] public string RepositoryId { get; set; }
        [DataMember(Name = "owner_uid")] public string OwnerId { get; set; }
        [DataMember(Name = "size_limit")] public long SizeLimit { get; set; }
        
        public static implicit operator StoragrRepository(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrRepository>(data);
    }
}