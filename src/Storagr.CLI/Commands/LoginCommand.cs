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
    public class LoginCommand : StoragrCommand
    {
        private class LocalOptions
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        
        public LoginCommand() 
            : base("login", StoragrConstants.LoginCommandDescription)
        {
            AddOption(new Option<string>(new[] {"--username", "-u"}, ""));
            AddOption(new Option<string>(new[] {"--password", "-p"}, ""));
            
            Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(Login);
        }

        private static async Task<int> Login(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();
            
            if (string.IsNullOrEmpty(options.Username) && (options.Username = console.ReadLine("Username")) is null)
                return Abort();

            if (string.IsNullOrEmpty(options.Password) && (options.Password = console.ReadPassword("Password")) is null)
                return Abort();

            if (string.IsNullOrEmpty(options.Username) || string.IsNullOrEmpty(options.Password))
            {
                return Error(console, "Username or password not provided!");
            }

            var client = host.GetStoragrClient();
            try
            {
                await console.Wait(token => client.Authenticate(options.Username, options.Password, token));
            }
            catch (TaskCanceledException)
            {
                return Abort();
            }
            catch (Exception exception)
            {
                return Error(console, exception);
            }

            Directory.CreateDirectory(StoragrConstants.TokenFilePath);
            await using var writer = File.CreateText(StoragrConstants.TokenFilePath);
            new JsonSerializer().Serialize(writer, new Dictionary<string, string> {{"token", client.Token}});

            return Success(console, "Login successful.");
        }
    }
}