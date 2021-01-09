using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public enum StoragrUpdateType
    {
        [EnumMember] User,
        [EnumMember] Repository
    }
    
    [DataContract]
    public class StoragrUpdateRequest
    {
        [DataMember(Name = "type", IsRequired = true)] public StoragrUpdateType Type { get; set; }
        [DataMember(Name = "updates", IsRequired = true)]
        public IDictionary<string, object> Updates { get; set; } = new Dictionary<string, object>();
        
        public static implicit operator StoragrUpdateRequest(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrUpdateRequest>(data);
    }
}