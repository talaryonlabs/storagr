using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Storagr.CLI
{
    public class LockOptions
    {
        public string Repository { get; set; }
        public string Path { get; set; }
    }
    
    public class LockCommand : Command
    {
        public LockCommand()
            : base("lock", StoragrConstants.LockCommandDescription)
        {
            AddOption(new RepositoryOption());
            AddArgument(new Argument<string>("path", "Path to lock")
            {
            });
            
            Handler = CommandHandler.Create<IHost, IConsole, LockOptions>(Lock);
        }

        private static async Task<int> Lock(IHost host, IConsole console, LockOptions options)
        {
            console.Out.WriteLine(options.Repository);
            console.Out.WriteLine(options.Path);
            return 0;
        }
    }
}