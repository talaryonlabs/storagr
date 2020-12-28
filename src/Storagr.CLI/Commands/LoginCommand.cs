using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Storagr.Client;

namespace Storagr.CLI
{
    public class LoginOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class LoginCommand : Command
    {
        public LoginCommand() : base("login", "Authenticate with username and password.")
        {
            AddOption(new Option<string>(new[] {"--username", "-u"}, ""));
            AddOption(new Option<string>(new[] {"--password", "-p"}, ""));
            
            Handler = CommandHandler.Create<IHost, IConsole, LoginOptions>(Login);
        }

        private static async Task Login(IHost host, IConsole console, LoginOptions options)
        {
            if (string.IsNullOrEmpty(options.Username))
            {
                console.Out.Write("Username: ");
                options.Username = Console.ReadLine();
            }

            if (string.IsNullOrEmpty(options.Password))
            {
                options.Password = "";
                
                console.Out.Write("Password: ");
                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = Console.ReadKey(true);

                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) && keyInfo.Key == ConsoleKey.C)
                        return;

                    if (keyInfo.Key == ConsoleKey.Backspace && options.Password.Length > 0)
                    {
                        console.Out.Write("\b \b");
                        options.Password = options.Password[0..^1];
                    }
                    else if(!char.IsControl(keyInfo.KeyChar))
                    {
                        console.Out.Write("*");
                        options.Password += keyInfo.KeyChar;
                    }
                    
                } while (keyInfo.Key != ConsoleKey.Enter);
                
                console.Out.WriteLine();
            }

            if (string.IsNullOrEmpty(options.Username) || string.IsNullOrEmpty(options.Password))
            {
                console.Error.WriteLine("Username or password not provided!");
                return;
            }

            console.Out.WriteLine();
            console.Out.WriteLine(options.Username);
            console.Out.WriteLine(options.Password);
            return;
            
            var client = host.Services.GetRequiredService<IStoragrClient>();

            Console.WriteLine("authenticate");
            Console.WriteLine(options.Username);
            Console.WriteLine(options.Password);

            var result = await client.Authenticate(options.Username, options.Password);
            
            Console.WriteLine($"Result: {result}");
            
            return;
        }
    }
}