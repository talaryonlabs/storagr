using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Shared;

namespace Storagr.CLI
{
    public class DeleteCommand : StoragrCommand
    {
        private class LocalOptions
        {
            public bool Force { get; set; }
            public string Repository { get; set; }
            public string Id { get; set; }
        }
        
        public DeleteCommand()
            : base("delete", StoragrConstants.DeleteCommandDescription)
        {
            var deleteUserCmd = new Command("user", StoragrConstants.DeleteUserCommandDescription)
            {
                new IdArgument(),
                new ForceOption()
            };
            var deleteRepositoryCmd = new Command("repository", StoragrConstants.DeleteRepositoryCommandDescription)
            {
                new IdArgument(),
                new ForceOption()
            };
            var deleteObjectCmd = new Command("object", StoragrConstants.DeleteObjectCommandDescription)
            {
                new IdArgument(),
                new RepositoryOption(),
                new ForceOption()
            };

            deleteUserCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(DeleteUser);
            deleteRepositoryCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(DeleteRepository);
            deleteObjectCmd.Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(DeleteObject);
            
            AddCommand(deleteUserCmd);
            AddCommand(deleteRepositoryCmd);
            AddCommand(deleteObjectCmd);
        }

        private static async Task<int> DeleteUser(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            if (options.Force is false)
                return Error(console, "You can only delete a user with the --force option.");
            
            try
            {
                await console.Wait(token => client
                    .User(options.Id)
                    .Delete(options.Force)
                    .RunAsync(token)
                );
            }
            catch (TaskCanceledException)
            {
                return Abort();
            }
            catch (Exception exception)
            {
                return Error(console, exception);
            }

            return Success(console, "User successfully deleted.");
        }

        private static async Task<int> DeleteRepository(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();
            
            if (options.Force is false)
                return Error(console, "You can only delete a repository with the --force option.");
            
            try
            {
                await console.Wait(token => client
                    .Repository(options.Id)
                    .Delete(options.Force)
                    .RunAsync(token)
                );
            }
            catch (TaskCanceledException)
            {
                return Abort();
            }
            catch (Exception exception)
            {
                return Error(console, exception);
            }
            
            return Success(console, "Repository successfully deleted.");
        }

        private static async Task<int> DeleteObject(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            if (options.Force is false)
                return Error(console, "You can only delete a object with the --force option.");
            
            try
            {
                await console.Wait(token => client
                    .Repository(options.Repository)
                    .Object(options.Id)
                    .Delete(options.Force)
                    .RunAsync(token)
                );
            }
            catch (TaskCanceledException)
            {
                return Abort();
            }
            catch (Exception exception)
            {
                return Error(console, exception);
            }
            
            return Success(console, "Object successfully deleted.");
        }
    }
}