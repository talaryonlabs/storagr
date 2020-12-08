using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrRef
    {
        [DataMember(Name = "name")] public string Name { get; set; }
    }
}