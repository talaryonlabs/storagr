using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Storagr.CLI
{
    public class GetOptions
    {
        public string Repository { get; set; }
        public string Id { get; set; }
    }
    
    public class GetCommand : Command
    {
        public GetCommand()
            : base("get", StoragrConstants.GetCommandDescription)
        {
            var getUserCmd = new Command("user", StoragrConstants.GetUserCommandDescription)
            {
                
            };
            var getRepositoryCmd = new Command("repository", StoragrConstants.GetRepositoryCommandDescription)
            {
                
            };
            var getObjectCmd = new Command("object", StoragrConstants.GetObjectCommandDescription)
            {
                new RepositoryOption()
            };
            var getLockCmd = new Command("lock", StoragrConstants.GetLockCommandDescription)
            {
                new RepositoryOption()
            };
            
            getUserCmd.Handler = CommandHandler.Create<IHost, GetOptions>(GetUser);
            getRepositoryCmd.Handler = CommandHandler.Create<IHost, GetOptions>(GetRepository);
            getObjectCmd.Handler = CommandHandler.Create<IHost, GetOptions>(GetObject);
            getLockCmd.Handler = CommandHandler.Create<IHost, GetOptions>(GetLock);

            AddCommand(getUserCmd);
            AddCommand(getRepositoryCmd);
            AddCommand(getObjectCmd);
            AddCommand(getLockCmd);
        }

        private static async Task<int> GetUser(IHost host, GetOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            var user = await client.GetUser(options.Id);
            
            // TODO what's next?
            
            return 0;
        }
        
        private static async Task<int> GetRepository(IHost host, GetOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            var repository = await client.GetRepository(options.Id);
            
            // TODO what's next?
            
            return 0;
        }
        
        private static async Task<int> GetObject(IHost host, GetOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            var obj = await client.GetObject(options.Repository, options.Id);
            
            // TODO what's next?
            
            return 0;
        }
        
        private static async Task<int> GetLock(IHost host, GetOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            var lck = await client.GetLock(options.Repository, options.Id);
            
            // TODO what's next?
            
            return 0;
        }
    }
}