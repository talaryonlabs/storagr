using System;
using System.Collections.Generic;
using System.CommandLine;
using Storagr.Shared;

namespace Storagr.CLI
{
    public class StoragrCommand : Command
    {
        protected StoragrCommand(string name, string description = null)
            : base(name, description)
        {
        }

        protected static int Success()
        {
            return 0;
        }

        protected static int Success(IConsoleService console, string message)
        {
            console.WriteSuccess(message);
            return Success();
        }

        protected static int Success<T>(IConsoleService console, T obj, bool outputAsJson=false) where T : new()
        {
            if (outputAsJson)
                console.WriteJson(obj);
            else
                console.WriteObject(obj);
            
            return Success();
        }
        
        protected static int Success<T>(IConsoleService console, IEnumerable<T> list, bool outputAsJson=false) where T : new()
        {
            if (outputAsJson)
                console.WriteJson(list);
            else
                console.WriteObject(list);
            
            return Success();
        }
        
        protected static int Success<T>(IConsoleService console, T obj, string message, bool outputAsJson=false) where T : new()
        {
            if (outputAsJson)
                console.WriteJson(obj);
            else
                console.WriteObject(obj);
            
            return Success(console, message);
        }
        
        protected static int Success<T>(IConsoleService console, IEnumerable<T> list, string message, bool outputAsJson=false) where T : new()
        {
            if (outputAsJson)
                console.WriteJson(list);
            else
                console.WriteObject(list);
            
            return Success(console, message);
        }

        protected static int Abort()
        {
            return -1;
        }
        
        protected static int Abort(IConsoleService console, string message)
        {
            console.WriteLine(message);
            return Abort();
        }

        protected static int Error(IConsoleService console, Exception exception)
        {
            if (exception is StoragrError error)
            {
                console.WriteError(error);
                return error.Code;
            }

            console.WriteError(exception);
            return 1;
        }

        protected static int Error(IConsoleService console, string message)
        {
            console.WriteError(message);
            return 1;
        }
    }
}