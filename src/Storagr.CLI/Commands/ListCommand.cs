using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Shared.Data;

namespace Storagr.CLI
{
    public class ListCommand : StoragrCommand
    {
        private class LocalOptions
        {
            public int Limit { get; set; }
            public string Cursor { get; set; }
            public string Repository { get; set; }
            public string IdPattern { get; set; }
            public string PathPattern { get; set; }
            public string UsernamePattern { get; set; }
        }
        
        public ListCommand() 
            : base("list", StoragrConstants.ListCommandDescription)
        {
            base.AddAlias("ls");

            var listUsersCmd = new Command("users", StoragrConstants.ListUsersCommandDescription)
            {
                new LimitOption(),
                new CursorOption(),
                new UsernamePatternOption()
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
            
            listUsersCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(ListUsers);
            listRepositoriesCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(ListRepositories);
            listObjectsCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(ListObjects);
            listLocksCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(ListLocks);
            
            AddCommand(listUsersCmd);
            AddCommand(listRepositoriesCmd);
            AddCommand(listObjectsCmd);
            AddCommand(listLocksCmd);
        }

        private static async Task<int> ListUsers(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();
            
            StoragrUserList list = default;
            try
            {
                await console.Wait(async token =>
                {
                    list = await client.GetUsers(new StoragrUserListArgs()
                    {
                        Cursor = options.Cursor,
                        Limit = options.Limit,
                        Username = options.UsernamePattern
                    }, token);
                });
            }
            catch (TaskCanceledException)
            {
                return Abort();
            }
            catch (Exception exception)
            {
                return Error(console, exception);
            }

            return Success(console, list.Items, globalOptions.AsJson);
        }
        
        private static async Task<int> ListRepositories(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            StoragrRepositoryList list = default;
            try
            {
                await console.Wait(async token =>
                {
                    list = await client.GetRepositories(new StoragrRepositoryListArgs()
                    {
                        Cursor = options.Cursor,
                        Limit = options.Limit,
                        Id = options.IdPattern
                    }, token);
                });
            }
            catch (TaskCanceledException)
            {
                return Abort();
            }
            catch (Exception exception)
            {
                return Error(console, exception);
            }
            
            return Success(console, list.Items, globalOptions.AsJson);
        }
        
        private static async Task<int> ListObjects(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            StoragrObjectList list = default;
            try
            {
                await console.Wait(async token =>
                {
                    list = await client.GetObjects(options.Repository, new StoragrObjectListQuery()
                    {
                        Cursor = options.Cursor,
                        Limit = options.Limit
                    }, token);
                });
            }
            catch (TaskCanceledException)
            {
                return Abort();
            }
            catch (Exception exception)
            {
                return Error(console, exception);
            }
            
            return Success(console, list.Items, globalOptions.AsJson);
        }
        
        private static async Task<int> ListLocks(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            StoragrLockList list = default;
            try
            {
                await console.Wait(async token =>
                {
                    list = await client.GetLocks(options.Repository, new StoragrLockListArgs()
                    {
                        Cursor = options.Cursor,
                        Limit = options.Limit,
                        LockId = options.IdPattern,
                        Path = options.PathPattern
                    }, token);
                });
            }
            catch (TaskCanceledException)
            {
                return Abort();
            }
            catch (Exception exception)
            {
                return Error(console, exception);
            }
            
            return Success(console, list.Items, globalOptions.AsJson);
        }
    }
}