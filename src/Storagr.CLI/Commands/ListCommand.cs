using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Shared.Data;

namespace Storagr.CLI
{
    public class ListOptions
    {
        public int Limit { get; set; }
        public string Cursor { get; set; }
        public string Repository { get; set; }
        public string IdPattern { get; set; }
        public string PathPattern { get; set; }
    }
    
    public class ListCommand : Command
    {
        public ListCommand() 
            : base("list", StoragrConstants.ListCommandDescription)
        {
            base.AddAlias("ls");

            var listUsersCmd = new Command("users", StoragrConstants.ListUsersCommandDescription)
            {
                new LimitOption(),
                new CursorOption(),
            };
            var listRepositoriesCmd = new Command("repositories", StoragrConstants.ListRepositoriesCommandDescription)
            {
                new LimitOption(),
                new CursorOption(),
            };
            var listObjectsCmd = new Command("objects", StoragrConstants.ListObjectsCommandDescription)
            {
                new RepositoryOption(),
                new LimitOption(),
                new CursorOption(),
            };
            var listLocksCmd = new Command("locks", StoragrConstants.ListLocksCommandDescription)
            {
                new RepositoryOption(),
                new LimitOption(),
                new CursorOption(),
                new IdPatternOption(),
                new PathPatternOption()
            };
            
            listUsersCmd.Handler = CommandHandler.Create<IHost, ListOptions>(ListUsers);
            listRepositoriesCmd.Handler = CommandHandler.Create<IHost, ListOptions>(ListRepositories);
            listObjectsCmd.Handler = CommandHandler.Create<IHost, ListOptions>(ListObjects);
            listLocksCmd.Handler = CommandHandler.Create<IHost, ListOptions>(ListLocks);
            
            AddCommand(listUsersCmd);
            AddCommand(listRepositoriesCmd);
            AddCommand(listObjectsCmd);
            AddCommand(listLocksCmd);
        }

        private static async Task<int> ListUsers(IHost host, ListOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            var list = await client.GetUsers(); // TODO use options

            // TODO whats next?
            
            return 0;
        }
        
        private static async Task<int> ListRepositories(IHost host, ListOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            var list = await client.GetRepositories(new StoragrRepositoryListArgs()
            {
                Cursor = options.Cursor,
                Limit = options.Limit,
                Id = options.IdPattern
            });
            
            // TODO whats next?
            
            return 0;
        }
        
        private static async Task<int> ListObjects(IHost host, ListOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            var list = await client.GetObjects(options.Repository, new StoragrObjectListQuery()
            {
                Cursor = options.Cursor,
                Limit = options.Limit
            });
            
            // TODO whats next?
            
            return 0;
        }
        
        private static async Task<int> ListLocks(IHost host, ListOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            var list = await client.GetLocks(options.Repository, new StoragrLockListArgs()
            {
                Cursor = options.Cursor,
                Limit = options.Limit,
                LockId = options.IdPattern,
                Path = options.PathPattern
            });
            
            // TODO whats next?
            
            return 0;
        }
    }
}