using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storagr.Shared.Data;

namespace Storagr.Shared
{
    [DataContract]
    public class StoragrError
    {
        [DataMember(Name = "code")] public int Code;
        [DataMember(Name = "request_id")] public string RequestId;
        [DataMember(Name = "message")] public string Message;
        [DataMember(Name = "documentation_url")] public string DocumentationUrl;

        public StoragrError()
            : this(StatusCodes.Status500InternalServerError, "Unkown Error")
        {
        }

        public StoragrError(string message) 
            : this(StatusCodes.Status500InternalServerError, message)
        {
        }

        public StoragrError(int code, string message)
        {
            Code = code;
            Message = message;
            DocumentationUrl = "https://github.com/talaryonstudios/storagr";

        }

        public static implicit operator ObjectResult(StoragrError error) => new ObjectResult(error)
        {
            StatusCode = error.Code
        };
    }

    [DataContract]
    public class NotImplementedError : StoragrError
    {
        public NotImplementedError() : base(StatusCodes.Status501NotImplemented, "Method not implemented.")
        {
        }
    }
    
    [DataContract]
    public class LockAlreadyExistsError : StoragrError
    {
        [DataMember(Name = "lock")] public StoragrLock Lock;
        
        public LockAlreadyExistsError(StoragrLock existingLock) : base(StatusCodes.Status409Conflict, "Lock already exists.")
        {
            Lock = existingLock;
        }
    }
    
    [DataContract]
    public class LockNotFoundError : StoragrError
    {
        public LockNotFoundError() : base(StatusCodes.Status404NotFound, "Lock not found.")
        {
            
        }
    }

    [DataContract]
    public class RepositoryAlreadyExistsError : StoragrError
    {
        [DataMember(Name = "repository")] public StoragrRepository Repository;
        
        public RepositoryAlreadyExistsError(StoragrRepository existingRepository) : base(StatusCodes.Status409Conflict, "Repository already exists.")
        {
            Repository = existingRepository;
        }
    }

    [DataContract]
    public class RepositoryNotFoundError : StoragrError
    {
        public RepositoryNotFoundError() : base(StatusCodes.Status404NotFound, "Repository not found.")
        {
            
        }
    }
    
    [DataContract]
    public class ObjectNotFoundError : StoragrError
    {
        public ObjectNotFoundError() : base(StatusCodes.Status404NotFound, "Object not found.")
        {
            
        }
    }
    
    [DataContract]
    public class AuthenticationError : StoragrError
    {
        public AuthenticationError() : base(StatusCodes.Status401Unauthorized, "Authentication failed. Username and Password correct?")
        {
        }
    }
    
    [DataContract]
    public class UsernameOrPasswordMissingError : StoragrError
    {
        public UsernameOrPasswordMissingError() : base(StatusCodes.Status422UnprocessableEntity, "Username or Password missing or empty!")
        {
        }
    }
    
    [DataContract]
    public class UserNotFoundError : StoragrError
    {
        public UserNotFoundError() : base(StatusCodes.Status404NotFound, "User not found.")
        {
        }
    }
}