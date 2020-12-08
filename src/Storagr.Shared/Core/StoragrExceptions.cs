using System;

namespace Storagr.Shared
{
    public class StoragrException : Exception
    {
        public StoragrException(string message) 
            : base(message)
        {
        }

        public StoragrException(StoragrError error)
            : this($"StoragrError[{error.Code}] {error.Message} (more information at {error.DocumentationUrl})")
        {
        }

        public static implicit operator StoragrException(StoragrError error) => 
            new StoragrException(error);
    }
    
    public class NotAuthorizedException : Exception
    {
        
    }
    
    public class NotPermittedException : Exception
    {
        
    }
    
    public class RepositoryNotFoundException : Exception
    {
        
    }

    public class RepositoryAlreadyExistsException : Exception
    {
        
    }

    public class ObjectNotFoundException : Exception
    {
        
    }
    
    public class ObjectAlreadyExistsException : Exception
    {
        
    }

    public class NotLockedException : Exception
    {
        
    }

    public class AlreadyLockedException : Exception
    {
        
    }
    
    public class UserAlreadyExistsException : Exception
    {
        
    }
    
    public class UserNotFoundException : Exception
    {
        
    }
}