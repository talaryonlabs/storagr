using System.Runtime.Serialization;
using Storagr.Shared.Data;

namespace Storagr
{
    [DataContract]
    public class StoragrError
    {
        public int Code;
        
        [DataMember(Name = "request_id")] public string RequestId;
        [DataMember(Name = "message")] public string Message;
        [DataMember(Name = "documentation_url")] public string DocumentationUrl;

        public StoragrError() : this(500, "Unkown Error")
        {
            DocumentationUrl = "https://github.com/talaryonstudios/storagr";
        }

        public StoragrError(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }

    [DataContract]
    public class LockAlreadyExistsError : StoragrError
    {
        [DataMember(Name = "lock")] public StoragrLock StoragrLock;
        
        public LockAlreadyExistsError(StoragrLock storagrLock) : base(409, "Lock already exists.")
        {
            StoragrLock = storagrLock;
        }
    }

    public class InvalidBatchOperationError : StoragrError
    {
        public InvalidBatchOperationError() : base(500, "Invalid operation - only 'upload' or 'download' are supported.")
        {
        }
    }

    public class RepositoryNotFoundError : StoragrError
    {
        public RepositoryNotFoundError() : base(404, "Repository not found.")
        {
            
        }
    }
    
    public class ObjectNotFoundError : StoragrError
    {
        public ObjectNotFoundError() : base(404, "Object not found.")
        {
            
        }
    }
}