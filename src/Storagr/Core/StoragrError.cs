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
        [DataMember(Name = "lock")] public StoragrLock Lock;
        
        public LockAlreadyExistsError(StoragrLock existingLock) : base(409, "Lock already exists.")
        {
            Lock = existingLock;
        }
    }

    public class InvalidBatchOperationError : StoragrError
    {
        public InvalidBatchOperationError() : base(500, "Invalid operation - only 'upload' or 'download' are supported.")
        {
        }
    }

    public class RepositoryAlreadyExistsError : StoragrError
    {
        [DataMember(Name = "repository")] public StoragrRepository Repository;
        
        public RepositoryAlreadyExistsError(StoragrRepository existingRepository) : base(409, "Repository already exists.")
        {
            Repository = existingRepository;
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