using System.Collections.Generic;
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

        public static implicit operator StoragrBatchError(StoragrError error) => new()
        {
            Code = error.Code,
            Message = error.Message
        };
    }
    
    [DataContract]
    public class StoragrBatchRequest
    {
        [DataMember(Name = "operation", IsRequired = true)]  public StoragrBatchOperation Operation  { get; set; }
        [DataMember(Name = "transfers")] public IEnumerable<string> Transfers  { get; set; } = new[] {"basic"};
        [DataMember(Name = "ref")] public StoragrRef Ref { get; set; }
        [DataMember(Name = "objects", IsRequired = true)] public IEnumerable<StoragrObject> Objects { get; set; }
    }

    [DataContract]
    public class StoragrBatchResponse
    {
        [DataMember(Name = "transfers", IsRequired = true)] public IEnumerable<string> Transfers { get; set; }
        [DataMember(Name = "objects", IsRequired = true)] public IEnumerable<StoragrBatchObject> Objects { get; set; }
        
        public static implicit operator StoragrBatchResponse(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrBatchResponse>(data);
    }
    
    [DataContract]
    public class StoragrBatchObject : StoragrObject
    {
        [DataMember(Name = "authenticated")] public bool Authenticated { get; set; }
        [DataMember(Name = "actions")] public StoragrActions Actions { get; set; }
        [DataMember(Name = "error")] public StoragrBatchError Error { get; set; }
    }
}