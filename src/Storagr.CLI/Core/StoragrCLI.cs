using System.CommandLine;

namespace Storagr.CLI
{
    // ReSharper disable once InconsistentNaming
    public class StoragrCLI : RootCommand
    {
        public StoragrCLI()
        {
            base.Name = "storagr";
            
            AddGlobalOption(new Option<string>("--host", "Storagr API endpoint"));
            
            AddCommand(new ConfigCommand());
            
            AddCommand(new LoginCommand());
            
            AddCommand(new NewCommand());
            AddCommand(new ListCommand());
            AddCommand(new GetCommand());
            AddCommand(new DeleteCommand());
            
            AddCommand(new LockCommand());
            AddCommand(new UnlockCommand());
        }
    }
}