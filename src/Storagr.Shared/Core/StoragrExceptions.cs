using System;

namespace Storagr.Shared
{
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
}