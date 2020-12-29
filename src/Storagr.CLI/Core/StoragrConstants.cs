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
        public const string ConfigCommandDescription = "Gets or sets a local config value.";
        
        public const string DeleteCommandDescription = "";
        public const string DeleteRepositoryCommandDescription = "";
        public const string DeleteUserCommandDescription = "";
        public const string DeleteObjectCommandDescription = "";
        
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
        
        public const string LockCommandDescription = "";
        public const string UnlockCommandDescription = "";
        public const string LoginCommandDescription = "";
        public const string TimelineCommandDescription = "";

        /**
         * Options
         */
        public const string HostOptionDescription = "";
        public const string TokenOptionDescription = "";
        
        public const string CursorOptionDescription = "";
        public const string ForceOptionDescription = "";
        public const string LimitOptionDescription = "";
        public const string RepositoryOptionDescription = "";

        public const string IdPatternOptionDescription = "";
        public const string PathPatternOptionDescription = "";
    }
}