using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrOwner
    {
        [DataMember(Name = "name")] public string Name;
    }
}