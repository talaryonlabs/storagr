using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using Storagr.Controllers.Models;

namespace Storagr
{
    [DataContract]
    public class Error
    {
        public int Code;
        
        [DataMember(Name = "request_id")] public string RequestId;
        [DataMember(Name = "message")] public string Message;
        [DataMember(Name = "documentation_url")] public string DocumentationUrl;

        public Error() : this(500, "Unkown Error")
        {
            DocumentationUrl = "https://github.com/talaryonstudios/storagr";
        }

        public Error(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }

    [DataContract]
    public class LockAlreadyExistsError : Error
    {
        [DataMember(Name = "lock")] public LockModel Lock;
        
        public LockAlreadyExistsError(LockModel lockModel) : base(409, "Lock already exists.")
        {
            Lock = lockModel;
        }
    }

    public class NotAuthorizedError : Error
    {
        public NotAuthorizedError() : base(403, "Not authorized")
        {
        }
    }

    public class InvalidBatchOperationError : Error
    {
        public InvalidBatchOperationError() : base(500, "Invalid operation - only 'upload' or 'download' are supported.")
        {
        }
    }

    public class RepositoryNotFoundError : Error
    {
        public RepositoryNotFoundError() : base(404, "Repository not found.")
        {
            
        }
    }
}