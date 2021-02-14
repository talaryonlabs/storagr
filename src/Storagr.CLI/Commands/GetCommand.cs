using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr;
using Storagr.Shared.Data;

namespace Storagr.CLI
{
    public class GetCommand : StoragrCommand
    {
        private class LocalOptions
        {
            public string Repository { get; set; }
            public string Id { get; set; }
            public string IdOrName { get; set; }
        }
        
        public GetCommand()
            : base("get", StoragrConstants.GetCommandDescription)
        {
            var getUserCmd = new Command("user", StoragrConstants.GetUserCommandDescription)
            {
                new IdOrNameArgument()
            };
            var getRepositoryCmd = new Command("repository", StoragrConstants.GetRepositoryCommandDescription)
            {
                new IdOrNameArgument()
            };
            var getObjectCmd = new Command("object", StoragrConstants.GetObjectCommandDescription)
            {
                new RepositoryOption(),
                new IdArgument()
            };
            var getLockCmd = new Command("lock", StoragrConstants.GetLockCommandDescription)
            {
                new RepositoryOption(),
                new IdArgument()
            };
            
            getUserCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(GetUser);
            getRepositoryCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(GetRepository);
            getObjectCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(GetObject);
            getLockCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(GetLock);

            AddCommand(getUserCmd);
            AddCommand(getRepositoryCmd);
            AddCommand(getObjectCmd);
            AddCommand(getLockCmd);
        }

        private static async Task<int> GetUser(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            StoragrUser user = default;
            try
            {
                await console.Wait(async token =>
                {
                    user = await client
                        .User(options.IdOrName)
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

            return Success(console, user, globalOptions.AsJson);
        }
        
        private static async Task<int> GetRepository(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            StoragrRepository repository = default;
            try
            {
                await console.Wait(async token =>
                {
                    repository = await client
                        .Repository(options.IdOrName)
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
            
            return Success(console, repository, globalOptions.AsJson);
        }
        
        private static async Task<int> GetObject(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            StoragrObject obj = default;
            try
            {
                await console.Wait(async token =>
                {
                    obj = await client
                        .Repository(options.Repository)
                        .Object(options.IdOrName)
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
            
            return Success(console, obj, globalOptions.AsJson);
        }
        
        private static async Task<int> GetLock(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            StoragrLock lck = default;
            try
            {
                await console.Wait(async token =>
                {
                    lck = await client
                        .Repository(options.Repository)
                        .Lock(options.Id)
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
            
            return Success(console, lck, globalOptions.AsJson);
        }
    }
}