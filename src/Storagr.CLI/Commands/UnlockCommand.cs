using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Shared.Data;

namespace Storagr.CLI
{
    public class UnlockCommand : StoragrCommand
    {
        private class LocalOptions
        {
            public bool Force { get; set; }
            public string Repository { get; set; }
            public string Id { get; set; }
        }
        
        public UnlockCommand()
            : base("unlock", StoragrConstants.UnlockCommandDescription)
        {
            AddOption(new RepositoryOption());
            AddOption(new ForceOption());
            AddArgument(new IdArgument());
            // AddOption(new Option<string>(new []{"--by-path"}, "Unlock by path")
            // {
            // });
            // AddOption(new Option<string>(new []{"--by-id"}, "Unlock by lock id")
            // {
            // });
            
            Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(Unlock);
        }

        private static async Task<int> Unlock(IHost host, LocalOptions options, GlobalOptions globalOptions)
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
                        .Delete(options.Force)
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

            return Success(console, lck, "Successfully unlocked.", globalOptions.AsJson);
        }
    }
}