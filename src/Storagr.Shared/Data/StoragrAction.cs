using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrAction
    {
        [DataMember(Name = "href", IsRequired = true)] public string Href;
        [DataMember(Name = "header")] public IDictionary<string, string> Header;
        [DataMember(Name = "expires_in")] public int ExpiresIn;
        [DataMember(Name = "expires_at")] public DateTime ExpiresAt;
    }

    [DataContract]
    public class StoragrActions
    {
        [DataMember(Name = "download")] public StoragrAction Download;
        [DataMember(Name = "upload")] public StoragrAction Upload;
        [DataMember(Name = "verify")] public StoragrAction Verify;
    }
}