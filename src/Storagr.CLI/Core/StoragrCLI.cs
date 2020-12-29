using System.CommandLine;

namespace Storagr.CLI
{
    // ReSharper disable once InconsistentNaming
    public class StoragrCLI : RootCommand
    {
        public StoragrCLI()
        {
            base.Name = "storagr";
            
            AddGlobalOption(new HostOption());
            AddGlobalOption(new TokenOption());
            
            AddCommand(new ConfigCommand());
            
            AddCommand(new LoginCommand());
            
            AddCommand(new NewCommand());
            AddCommand(new ListCommand());
            AddCommand(new GetCommand());
            AddCommand(new DeleteCommand());
            // TODO AddCommand(new TimelineCommand());
            
            AddCommand(new LockCommand());
            AddCommand(new UnlockCommand());
        }
    }
}