using System;

namespace Storagr.Shared
{
    public class RepositoryNotFoundException : Exception
    {
        
    }

    public class RepositoryExistsException : Exception
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