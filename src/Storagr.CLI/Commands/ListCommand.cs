using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Data;

namespace Storagr.CLI
{
    public class ListCommand : StoragrCommand
    {
        private class LocalOptions
        {
            public int Limit { get; set; }
            public int Skip { get; set; }
            public string Cursor { get; set; }
            public string Repository { get; set; }
            public string IdPattern { get; set; }
            public string PathPattern { get; set; }
            public string UsernamePattern { get; set; }
            public string NamePattern { get; set; }
            public bool IsAdmin { get; set; }
            public bool IsEnabled { get; set; }
            public string Owner { get; set; }
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
                    list = (StoragrUserList)await client
                        .Users()
                        .Skip(options.Skip)
                        .SkipUntil(options.Cursor)
                        .Take(options.Limit)
                        .Where(u => u
                            .Username(options.UsernamePattern)
                            .IsAdmin(options.IsAdmin)
                            .IsEnabled(options.IsEnabled)
                        )
                        .RunAsync(token);
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
                    list = (StoragrRepositoryList)await client
                        .Repositories()
                        .Skip(options.Skip)
                        .SkipUntil(options.Cursor)
                        .Take(options.Limit)
                        .Where(r => r
                            .Name(options.NamePattern)
                            .Owner(options.Owner)
                        )
                        .RunAsync(token);
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
                    list = (StoragrObjectList)await client
                        .Repository(options.Repository)
                        .Objects()
                        .Skip(options.Skip)
                        .SkipUntil(options.Cursor)
                        .Take(options.Limit)
                        .RunAsync(token);
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
                    list = (StoragrLockList)await client
                        .Repository(options.Repository)
                        .Locks()
                        .Skip(options.Skip)
                        .SkipUntil(options.Cursor)
                        .Take(options.Limit)
                        .Where(l => l
                            .Id(options.IdPattern)
                            .Path(options.PathPattern)
                        )
                        .RunAsync(token);
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