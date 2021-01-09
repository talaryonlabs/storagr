using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Shared.Data;

namespace Storagr.CLI
{
    public class UsernameOption : Option<string>
    {
        public UsernameOption()
            : base(new[] {"--username"}, StoragrConstants.UsernameOptionDescription)
        {
        }
    }

    public class UpdateCommand : StoragrCommand
    {
        private class LocalOptions
        {
            public string IdOrName { get; set; }

            public string Username { get; set; }
            public string Name { get; set; }
            public string Owner { get; set; }
            public ulong SizeLimit { get; set; }
        }

        public UpdateCommand()
            : base("update", StoragrConstants.UpdateCommandDescription)
        {
            var getUserCmd = new Command("user", StoragrConstants.UpdateUserCommandDescription)
            {
                new IdOrNameArgument(),
                new Option<string>("--username"),
                new Option("--enable"),
                new Option("--disable"),
                new Option("--change-password")
            };
            var getRepositoryCmd = new Command("repository", StoragrConstants.UpdateRepositoryCommandDescription)
            {
                new IdOrNameArgument(),
                new Option<string>("--name"),
                new Option<string>("--owner"),
                new SizeLimitOption()
            };

            getUserCmd.Handler = CommandHandler.Create<IHost, InvocationContext, LocalOptions, GlobalOptions>(UpdateUser);
            getRepositoryCmd.Handler =
                CommandHandler.Create<IHost, InvocationContext, LocalOptions, GlobalOptions>(UpdateRepository);

            AddCommand(getUserCmd);
            AddCommand(getRepositoryCmd);
        }

        private static async Task<int> UpdateUser(IHost host, InvocationContext context, LocalOptions options,
            GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();
            var updater = client.UpdateUser(options.IdOrName);
            var user = default(StoragrUser);

            if (context.ParseResult.HasOption("--username"))
                updater.SetUsername(options.Username);
            
            if (context.ParseResult.HasOption("--enable"))
                updater.SetEnabled(true);
            else if (context.ParseResult.HasOption("--disable"))
                updater.SetEnabled(false);

            if (context.ParseResult.HasOption("--change-password"))
            {
                var password = console.ReadPassword("New Password");
                updater.SetPassword(password);
            }

            try
            {
                await console.Wait(async token =>
                {
                    user = await updater.Update(token); 
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

            return Success(console, user, "User successfully updated.", globalOptions.AsJson);
        }

        private static async Task<int> UpdateRepository(IHost host, InvocationContext context, LocalOptions options,
            GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();
            var updater = client.UpdateRepository(options.IdOrName);
            var repository = default(StoragrRepository);

            if (context.ParseResult.HasOption("--name"))
                updater.SetName(options.Name);
            
            if (context.ParseResult.HasOption("--owner"))
                updater.SetOwner(options.Owner);
            
            if (context.ParseResult.HasOption("--size-limit"))
                updater.SetSizeLimit(options.SizeLimit);
            
            try
            {
                await console.Wait(async token =>
                {
                    repository = await updater.Update(token); 
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

            return Success(console, repository, "Repository successfully updated.", globalOptions.AsJson);
        }
    }
}