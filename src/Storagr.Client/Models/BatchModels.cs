using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Storagr.Client.Models
{
    [DataContract]
    public enum BatchOperation
    {
        [EnumMember] Download,
        [EnumMember] Upload,
    }
    
    [DataContract]
    public class BatchModel
    {
        [DataMember(Name = "oid", IsRequired = true)] public string ObjectId;
        [DataMember(Name = "size", IsRequired = true)] public long Size;
    }
    
    [DataContract]
    public class BatchError
    {
        [DataMember(Name = "code", IsRequired = true)] public int Code;
        [DataMember(Name = "message", IsRequired = true)] public string Message;
    }
    
    [DataContract]
    public class BatchResponseModel : BatchModel
    {
        [DataMember(Name = "authenticated")] public bool Authenticated;
        [DataMember(Name = "actions")] public BatchActionList Actions; 
        [DataMember(Name = "error")] public BatchError Error;
    }
    
    [DataContract]
    public class BatchAction
    {
        [DataMember(Name = "href", IsRequired = true)] public string Href { get; set; }
        [DataMember(Name = "header")] public IDictionary<string, string> Header { get; set; }
        [DataMember(Name = "expires_in")] public int ExpiresIn { get; set; }
        [DataMember(Name = "expires_at")] public DateTime ExpiresAt { get; set; }
    }

    [DataContract]
    public class BatchActionList
    {
        [DataMember(Name = "download")] public BatchAction Download { get; set; }
        [DataMember(Name = "upload")] public BatchAction Upload { get; set; }
        [DataMember(Name = "verify")] public BatchAction Verify { get; set; }
    }
    
    [DataContract]
    public class BatchRequest
    {
        [DataMember(Name = "operation", IsRequired = true)]  public BatchOperation Operation;
        [DataMember(Name = "transfers")] public IEnumerable<string> Transfers = new[] {"basic"};
        [DataMember(Name = "ref")] public RefData Ref;
        [DataMember(Name = "objects", IsRequired = true)] public IEnumerable<BatchModel> Objects;
    }

    [DataContract]
    public class BatchResponse
    {
        [DataMember(Name = "transfers", IsRequired = true)] public IEnumerable<string> Transfers;
        [DataMember(Name = "objects", IsRequired = true)] public IEnumerable<BatchResponseModel> Objects;
    }
    
    

    [DataContract]
    public class BatchHeader
    {
        [DataMember] public string Name;
        [DataMember] public string Value;

        public BatchHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static BatchHeader Authorization(string value) => new BatchHeader("Authorization", value);
        
        public static implicit operator KeyValuePair<string, string>(BatchHeader header) => new KeyValuePair<string, string>(header.Name, header.Value);
    }
    
    
    
    
}