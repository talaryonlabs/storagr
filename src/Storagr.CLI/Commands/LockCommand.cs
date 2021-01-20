using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.CLI
{
    public class LockCommand : StoragrCommand
    {
        private class LocalOptions
        {
            public string Repository { get; set; }
            public string Path { get; set; }
        }

        public LockCommand()
            : base("lock", StoragrConstants.LockCommandDescription)
        {
            AddOption(new RepositoryOption());
            AddArgument(new Argument<string>("path", "Path to lock")
            {
            });
            
            Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(Lock);
        }

        private static async Task<int> Lock(IHost host, LocalOptions options, GlobalOptions globalOptions)
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
                        .Lock(options.Path)
                        .Create()
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
            
            return Success(console, lck, "Successfully locked.", globalOptions.AsJson);
        }
    }
}