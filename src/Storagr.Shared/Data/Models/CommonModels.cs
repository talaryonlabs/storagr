using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class RefData
    {
        [DataMember(Name = "name")] public string Name;
    }

    [DataContract]
    public class OwnerData
    {
        [DataMember(Name = "name")] public string Name;
    }
}