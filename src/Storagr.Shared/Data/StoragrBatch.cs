﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Shared.Data
{
    [DataContract]
    public enum StoragrBatchOperation
    {
        [EnumMember] Download,
        [EnumMember] Upload,
    }
    
    [DataContract]
    public class StoragrBatchError
    {
        [DataMember(Name = "code", IsRequired = true)] public int Code;
        [DataMember(Name = "message", IsRequired = true)] public string Message;

        public static implicit operator StoragrBatchError(StoragrError error) => new StoragrBatchError()
        {
            Code = error.Code,
            Message = error.Message
        };
    }
    
    [DataContract]
    public class StoragrBatchRequest
    {
        [DataMember(Name = "operation", IsRequired = true)]  public StoragrBatchOperation Operation ;
        [DataMember(Name = "transfers")] public IEnumerable<string> Transfers = new[] {"basic"};
        [DataMember(Name = "ref")] public StoragrRef Ref;
        [DataMember(Name = "objects", IsRequired = true)] public IEnumerable<StoragrObject> Objects;
    }

    [DataContract]
    public class StoragrBatchResponse
    {
        [DataMember(Name = "transfers", IsRequired = true)] public IEnumerable<string> Transfers;
        [DataMember(Name = "objects", IsRequired = true)] public IEnumerable<StoragrBatchResponseObject> Objects;
    }
    
    [DataContract]
    public class StoragrBatchResponseObject : StoragrObject
    {
        [DataMember(Name = "authenticated")] public bool Authenticated;
        [DataMember(Name = "actions")] public StoragrActions Actions;
        [DataMember(Name = "error")] public StoragrBatchError Error;
    }
}