using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoreInformation
    {
        [DataMember(Name = "available_space")] public long AvailableSpace;
    }
}