using System;
using System.IO;

namespace Storagr.CLI
{
    // TODO create descriptions
    public static class StoragrConstants
    {
        

        public static string TokenFilePath => Path.Combine(new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".storagr",
            "token-file"
        });


        /**
         * Commands
         */
        public const string TestCommandDescription = "Just for testing";
        
        public const string ConfigCommandDescription = "Gets or sets a local config value.";
        
        public const string DeleteCommandDescription = "Deletes a resource";
        public const string DeleteRepositoryCommandDescription = "Deletes a repository";
        public const string DeleteUserCommandDescription = "Deletes a user";
        public const string DeleteObjectCommandDescription = "Deletes an object from a repository";
        
        public const string GetCommandDescription = "";
        public const string GetUserCommandDescription = "";
        public const string GetRepositoryCommandDescription = "";
        public const string GetObjectCommandDescription = "";
        public const string GetLockCommandDescription = "";
        
        public const string ListCommandDescription = "";
        public const string ListUsersCommandDescription = "";
        public const string ListRepositoriesCommandDescription = "";
        public const string ListObjectsCommandDescription = "";
        public const string ListLocksCommandDescription = "";
        
        public const string NewCommandDescription = "";
        public const string NewUserCommandDescription = "";
        public const string NewRepositoryCommandDescription = "";
        
        public const string UpdateCommandDescription = "";
        public const string UpdateUserCommandDescription = "";
        public const string UpdateRepositoryCommandDescription = "";
        
        public const string LockCommandDescription = "";
        public const string UnlockCommandDescription = "";
        public const string LoginCommandDescription = "";
        public const string TimelineCommandDescription = "";

        /**
         * Options
         */
        public const string HostOptionDescription = "API Endpoint [http|s://]hostname[:port] (protocol and port optional)";
        public const string AsJsonOptionDescription = "Outputs the result in json format";
        public const string TokenOptionDescription = "";
        
        public const string WithResultOptionDescription = "Shows the resulting object of your request.";
        
        public const string CursorOptionDescription = "";
        public const string ForceOptionDescription = "";
        public const string LimitOptionDescription = "";
        public const string RepositoryOptionDescription = "";
        public const string UsernameOptionDescription = "";
        public const string SizeLimitOptionDescription = "";

        public const string IdPatternOptionDescription = "";
        public const string PathPatternOptionDescription = "";
        public const string UsernamePatternOptionDescription = "";
        
        /**
         * Arguments
         */
        public const string IdArgumentDescription = "Identifier of the requested resource";
        public const string IdOrNameArgumentDescription = "Id or (User-/Repository-)name of the requested resource";
    }
} 