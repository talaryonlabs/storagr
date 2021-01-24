using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Data;

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
            var user = default(StoragrUser);

            try
            {
                await console.Wait(async token =>
                {
                    user = await client
                        .User(options.IdOrName)
                        .Update()
                        .With(u =>
                        {
                            if (context.ParseResult.HasOption("--username"))
                                u.Username(options.Username);
            
                            if (context.ParseResult.HasOption("--enable"))
                                u.IsEnabled(true);
                            else if (context.ParseResult.HasOption("--disable"))
                                u.IsEnabled(false);

                            if (context.ParseResult.HasOption("--change-password"))
                            {
                                var password = console.ReadPassword("New Password");
                                u.Password(password);
                            }
                        })
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

            return globalOptions.WithResult
                ? Success(console, user, "User successfully updated.", globalOptions.AsJson)
                : Success(console, "User successfully updated.");
        }

        private static async Task<int> UpdateRepository(IHost host, InvocationContext context, LocalOptions options,
            GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();
            var repository = default(StoragrRepository);
            
            try
            {
                await console.Wait(async token =>
                {
                    repository = await client
                        .Repository(options.IdOrName)
                        .Update()
                        .With(r =>
                        {
                            if (context.ParseResult.HasOption("--name"))
                                r.Name(options.Name);
            
                            if (context.ParseResult.HasOption("--owner"))
                                r.Owner(options.Owner);
            
                            if (context.ParseResult.HasOption("--size-limit"))
                                r.SizeLimit(options.SizeLimit);
                        })
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

            return globalOptions.WithResult
                ? Success(console, repository, "Repository successfully updated.", globalOptions.AsJson)
                : Success(console, "Repository successfully updated.");
        }
    }
}