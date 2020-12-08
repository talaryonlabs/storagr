using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrObject
    {
        [DataMember(Name = "oid")] public string ObjectId { get; set; }
        [DataMember(Name = "size")] public long Size { get; set; }
        
        public static implicit operator StoragrObject(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrObject>(data);
    }
}