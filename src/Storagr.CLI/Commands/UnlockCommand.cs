using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Storagr.CLI
{
    public class UnlockOptions
    {
        public string Repository { get; set; }
    }
    
    public class UnlockCommand : Command
    {
        public UnlockCommand()
            : base("unlock", StoragrConstants.UnlockCommandDescription)
        {
            AddOption(new RepositoryOption());
            AddOption(new ForceOption());
            // AddOption(new Option<string>(new []{"--by-path"}, "Unlock by path")
            // {
            // });
            // AddOption(new Option<string>(new []{"--by-id"}, "Unlock by lock id")
            // {
            // });
            
            Handler = CommandHandler.Create<IHost, IConsole, UnlockOptions>(Unlock);
        }

        private static async Task<int> Unlock(IHost host, IConsole console, UnlockOptions options)
        {
            return 0;
        }
    }
}