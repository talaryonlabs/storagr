using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrRepository
    {
        [DataMember(Name = "rid", IsRequired = true)] public string RepositoryId { get; set; }
        [DataMember(Name = "name", IsRequired = true)] public string Name { get; set; }
        [DataMember(Name = "owner")] public string Owner { get; set; }
        [DataMember(Name = "size_limit")] public ulong SizeLimit { get; set; }
        
        public static implicit operator StoragrRepository(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrRepository>(data);
    }
}