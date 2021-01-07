using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.CLI
{
    public class NewCommand : StoragrCommand
    {
        private class LocalOptions
        {
            /**
             * User
             */
            public string Username { get; set; }
            public bool IsAdmin { get; set; } = false;
            public bool IsEnabled { get; set; } = false;

            /**
             * Repository
             */
            public string Name { get; set; }
            public ulong SizeLimit { get; set; } = 10737418240; // 10G
            public string Owner { get; set; } = null;
        }
        
        public NewCommand()
            : base("new", StoragrConstants.NewCommandDescription)
        {
            AddGlobalOption(new WithResultOption());
            
            var newUserCmd = new Command("user", StoragrConstants.NewUserCommandDescription)
            {
                new Argument("username"),
                new Option("--is-admin"),
                new Option("--is-enabled")
            };
            var newRepositoryCmd = new Command("repository", StoragrConstants.NewRepositoryCommandDescription)
            {
                new Argument("name")
                {
                    Description = "Name of the new repository"
                },
                new SizeLimitOption(),
                new Option<string>("--owner", "UserId or Username")
            };
            
            newUserCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(NewUser);
            newRepositoryCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(NewRepository);
            
            AddCommand(newUserCmd);
            AddCommand(newRepositoryCmd);
        }

        private static async Task<int> NewUser(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();
            var password = default(string);
            var user = new StoragrUser()
            {
                IsEnabled = options.IsEnabled,
                IsAdmin = options.IsAdmin,
                
                Username = options.Username
            };
            
            if ((password = console.ReadPassword("Password")) is null)
                return Abort();

            if (string.IsNullOrEmpty(password))
                return Error(console, "No password provided - no user created");
            
            try
            {
                await console.Wait(async token =>
                {
                    user = await client.CreateUser(user, password);
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
                ? Success(console, user, "User successfully created.", globalOptions.AsJson)
                : Success(console, "User successfully created.");
        }
        
        private static async Task<int> NewRepository(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();
            var repository = new StoragrRepository()
            {
                RepositoryId = options.Name,
                SizeLimit = options.SizeLimit,
                OwnerId = options.Owner
            };
            
            try
            {
                await console.Wait(async token =>
                {
                    repository = await client.CreateRepository(repository, token);
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
                ? Success(console, repository, "Repository successfully created.", globalOptions.AsJson)
                : Success(console, "Repository successfully created.");
        }
    }
}