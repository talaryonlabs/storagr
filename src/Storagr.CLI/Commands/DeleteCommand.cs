using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storagr.Shared;

namespace Storagr.CLI
{
    public class DeleteOptions
    {
        public bool Force { get; set; }
        public string Repository { get; set; }
        public string Id { get; set; }
    }
    
    public class DeleteCommand : Command
    {
        public DeleteCommand()
            : base("delete", StoragrConstants.DeleteCommandDescription)
        {
            var deleteUserCmd = new Command("user", StoragrConstants.DeleteUserCommandDescription)
            {
                new ForceOption()
            };
            var deleteRepositoryCmd = new Command("repository", StoragrConstants.DeleteRepositoryCommandDescription)
            {
                new ForceOption()
            };
            var deleteObjectCmd = new Command("object", StoragrConstants.DeleteObjectCommandDescription)
            {
                new RepositoryOption(),
                new ForceOption()
            };

            deleteUserCmd.Handler = CommandHandler.Create<IHost, DeleteOptions>(DeleteUser);
            deleteRepositoryCmd.Handler = CommandHandler.Create<IHost, DeleteOptions>(DeleteRepository);
            deleteObjectCmd.Handler = CommandHandler.Create<IHost, DeleteOptions>(DeleteObject);
            
            AddCommand(deleteUserCmd);
            AddCommand(deleteRepositoryCmd);
            AddCommand(deleteObjectCmd);
        }

        private static async Task<int> DeleteUser(IHost host, DeleteOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            try
            {
                await client.DeleteUser(options.Id);
            }
            catch (StoragrException e)
            {
                console.WriteError(e);
                return e.Code;
            }
            catch (Exception e)
            {
                console.WriteError(e);
                return -1;
            }
            // TODO console output on success
            
            return 0;
        }

        private static async Task<int> DeleteRepository(IHost host, DeleteOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();
            
            try
            {
                await client.DeleteRepository(options.Id);
            }
            catch (StoragrException e)
            {
                console.WriteError(e);
                return e.Code;
            }
            catch (Exception e)
            {
                console.WriteError(e);
                return -1;
            }
            // TODO console output on success
            
            return 0;
        }

        private static async Task<int> DeleteObject(IHost host, DeleteOptions options)
        {
            var console = host.GetConsole();
            var client = host.GetStoragrClient();

            try
            {
                await client.DeleteObject(options.Repository, options.Id);
            }
            catch (StoragrException e)
            {
                console.WriteError(e);
                return e.Code;
            }
            catch (Exception e)
            {
                console.WriteError(e);
                return -1;
            }
            // TODO console output on success
            
            return 0;
        }
    }
}