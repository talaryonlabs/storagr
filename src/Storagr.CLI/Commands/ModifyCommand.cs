using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Storagr.CLI
{
    public class UsernameOption : Option<string>
    {
        public UsernameOption()
            : base(new[] {"--username"}, StoragrConstants.UsernameOptionDescription)
        {
        }
    }

    public class ModifyCommand : StoragrCommand
    {
        private class LocalOptions
        {
            public string Id { get; set; }
            
            public string Username { get; set; }
            
            public bool SetPassword { get; set; }
            
            public string RenameTo { get; set; }
            
            public long SizeLimit { get; set; }
        }
        
        public ModifyCommand()
            : base("modify", StoragrConstants.ModifyCommandDescription)
        {
            var getUserCmd = new Command("user", StoragrConstants.ModifyUserCommandDescription)
            {
                new IdArgument(),
                new UsernameOption(),
                new Option("--set-password")
            };
            var getRepositoryCmd = new Command("repository", StoragrConstants.ModifyRepositoryCommandDescription)
            {
                new IdArgument(),
                new Option<string>("--rename-to"),
                new SizeLimitOption()
            };
            
            getUserCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(ModifyUser);
            getRepositoryCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(ModifyRepository);

            AddCommand(getUserCmd);
            AddCommand(getRepositoryCmd);
        }
        
        private static async Task<int> ModifyUser(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            try
            {
                await console.Wait(async token =>
                {
                    await Task.Delay(100, token); // TODO
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

            return Success(console, "User successfully created.");
        }
        
        private static async Task<int> ModifyRepository(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            try
            {
                await console.Wait(async token =>
                {
                    await Task.Delay(100, token); // TODO
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

            return Success(console, "Repository successfully created.");
        }
    }
}