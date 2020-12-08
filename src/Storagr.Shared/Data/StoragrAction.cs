using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrAction
    {
        [DataMember(Name = "href", IsRequired = true)] public string Href { get; set; }
        [DataMember(Name = "header")] public IDictionary<string, string> Header { get; set; }
        [DataMember(Name = "expires_in")] public int ExpiresIn { get; set; }
        [DataMember(Name = "expires_at")] public DateTime ExpiresAt { get; set; }
    }

    [DataContract]
    public class StoragrActions
    {
        [DataMember(Name = "download")] public StoragrAction Download { get; set; }
        [DataMember(Name = "upload")] public StoragrAction Upload { get; set; }
        [DataMember(Name = "verify")] public StoragrAction Verify { get; set; }
    }
}