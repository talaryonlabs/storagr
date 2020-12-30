using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr.Shared
{
    [DataContract]
    public class StoragrError : Exception
    {
        [DataMember(Name = "code")] public int Code { get; set; }
        [DataMember(Name = "request_id")] public string RequestId { get; set; }
        [DataMember(Name = "message")] public new string Message { get; set; }
        [DataMember(Name = "documentation_url")] public string DocumentationUrl { get; set; }

        public StoragrError()
            : this(StatusCodes.Status500InternalServerError, "Unkown Error")
        {
        }

        public StoragrError(string message) 
            : this(StatusCodes.Status500InternalServerError, message)
        {
        }

        public StoragrError(Exception e)
            : this(e.Message)
        {

        }

        public StoragrError(int code, string message)
        {
            Code = code;
            Message = message;
            DocumentationUrl = "https://github.com/talaryonstudios/storagr";

        }

        public static implicit operator StoragrError(byte[] data) =>
            StoragrHelper.DeserializeObject<StoragrError>(data);
    }
}