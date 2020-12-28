using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Storagr.CLI
{
    public class ConfigArguments
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    
    public class ConfigCommand : Command
    {
        private readonly Command[] _commands = {
            new Command("host", ""),
        };
        
        
        public ConfigCommand()
            : base("config", "Gets or sets a local config value.")
        {
            var nameArg = new Argument<string>("name");
            var valueArg = new Argument<string>("value");

            var setCmd = new Command("set", "Sets a config value")
            {
                Handler = CommandHandler.Create<IHost, IConsole, ConfigArguments>(Set)
            };
            var getCmd = new Command("get", "Gets a config value")
            {
                Handler = CommandHandler.Create<IHost, IConsole, ConfigArguments>(Get)
            };
            
            foreach (var cmd in _commands)
            {
                cmd.AddArgument(valueArg);
                setCmd.AddCommand(cmd);
            }        
            
            
            
            getCmd.AddArgument(nameArg);

            var listCmd = new Command("list", "List all config entries")
            {
                Handler = CommandHandler.Create<IHost, IConsole>(List),
            };
            listCmd.AddAlias("ls");
            
            AddCommand(listCmd);
            AddCommand(getCmd);
            AddCommand(setCmd);
        }

        private static void List(IHost host, IConsole console)
        {
            var service = host.Services.GetService<IConfigService>() ?? throw new NullReferenceException();
            foreach (var (key, value) in service.GetAll())
            {
                console.Out.WriteLine($"{key}: {value}");
            }
        }

        private static void Set(IHost host, IConsole console, ConfigArguments args)
        {
            var service = host.Services.GetService<IConfigService>() ?? throw new NullReferenceException();

            if (!service.Exists(args.Name))
            {
                console.Error.WriteLine($"Name ´{args.Name}´ not found.");
                return;
            }

            service.Set(args.Name, args.Value);
            console.Out.WriteLine($"{args.Name} is now: {args.Value}");
        }

        private static void Get(IHost host, IConsole console, ConfigArguments args)
        {
            var service = host.Services.GetService<IConfigService>() ?? throw new NullReferenceException();
            if (!service.Exists(args.Name))
            {
                console.Error.WriteLine($"Name ´{args.Name}´ not found.");
                return;
            }
            console.Out.WriteLine($"{args.Name}: {service.Get(args.Name)}");
        }
    }
}