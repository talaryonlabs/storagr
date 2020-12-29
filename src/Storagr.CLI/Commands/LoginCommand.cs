using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Storagr.Shared;

namespace Storagr.CLI
{
    public class LoginOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class LoginCommand : Command
    {
        public LoginCommand() 
            : base("login", StoragrConstants.LoginCommandDescription)
        {
            AddOption(new Option<string>(new[] {"--username", "-u"}, ""));
            AddOption(new Option<string>(new[] {"--password", "-p"}, ""));
            
            Handler = CommandHandler.Create<IHost, LoginOptions>(Login);
        }

        private static async Task<int> Login(IHost host, LoginOptions options)
        {
            var console = host.GetConsole();
            
            if (string.IsNullOrEmpty(options.Username))
            {
                options.Username = console.ReadLine("Username");
            }

            if (string.IsNullOrEmpty(options.Password) && (options.Password = console.ReadPassword("Password")) is null)
                return 0; // aborting

            if (string.IsNullOrEmpty(options.Username) || string.IsNullOrEmpty(options.Password))
            {
                console.WriteError("Username or password not provided!");
                return 1;
            }

            var client = host.GetStoragrClient();
            try
            {
                await client.Authenticate(options.Username, options.Password);
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

            await using var writer = File.CreateText(StoragrConstants.TokenFilePath);
            new JsonSerializer().Serialize(writer, new Dictionary<string, string> {{"token", client.Token}});

            // TODO console output on success
            return 0;
        }
    }
}