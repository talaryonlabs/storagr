using System.Runtime.Serialization;

namespace Storagr.Client.Models
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