using System;
using System.CommandLine;
using System.CommandLine.Invocation;

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
            AddGlobalOption(new AsJsonOption());
            
            AddCommand(new Command("help", "Prints this help page")
            {
                Handler = CommandHandler.Create(() =>
                {
                    this.Invoke("--help");
                })
            });
            AddCommand(new Command("version", "Prints version information")
            {
                Handler = CommandHandler.Create(() =>
                {
                    this.Invoke("--version");
                })
            });
            
            AddCommand(new TestCommand());
            AddCommand(new ConfigCommand());
            
            AddCommand(new LoginCommand());
            
            AddCommand(new NewCommand());
            AddCommand(new ListCommand());
            AddCommand(new GetCommand());
            AddCommand(new DeleteCommand());
            AddCommand(new TimelineCommand());
            
            AddCommand(new LockCommand());
            AddCommand(new UnlockCommand());
        }
    }
}