using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

namespace Storagr.Shared.Data
{
    [DataContract]
    public class StoragrLog
    {
        [DataMember(Name = "id")] public int LogId { get; set; }
        [DataMember(Name = "date")] public DateTime Date { get; set; }
        [DataMember(Name = "level")] public LogLevel Level { get; set; }
        [DataMember(Name = "category")] public string Category { get; set; }
        [DataMember(Name = "message")] public string Message { get; set; }
        [DataMember(Name = "exception")] public string Exception { get; set; }
    }
}