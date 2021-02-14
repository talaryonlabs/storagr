using System;
using System.Runtime.Serialization;

namespace Storagr
{
    [DataContract]
    public class StoragrError : Exception
    {
        [DataMember(Name = "code")] public int Code { get; set; }
        [DataMember(Name = "request_id")] public string RequestId { get; set; }
        [DataMember(Name = "message")] public new string Message { get; set; }
        [DataMember(Name = "documentation_url")] public string DocumentationUrl { get; set; }
        [DataMember(Name = "stack_trace")] private new string StackTrace { get; set; }

        public StoragrError()
        {
            DocumentationUrl = "https://github.com/talaryonstudios/storagr";
        }

        public StoragrError(int code, string message)
            : this()
        {
            Code = code;
            Message = message;
        }

        public StoragrError(int code, Exception e)
            : this(code, e.Message)
        {
            StackTrace = e.StackTrace;
        }
    }
}