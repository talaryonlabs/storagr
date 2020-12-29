﻿using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Storagr.Client;

namespace Storagr.CLI
{
    public class Program
    {
        public static Task<int> Main(string[] args) =>
            new CommandLineBuilder(new StoragrCLI())
                .UseHost(host =>
                {
                    host.ConfigureAppConfiguration(configuration =>
                    {
                        configuration
                            .AddJsonFile("appsettings.json", true)
                            .AddConfigFile()
                            .AddTokenFile()
                            .AddCommandLine(args);
                    });
                    host.ConfigureServices((context, services) =>
                    {
                        services
                            .AddStoragrClient(options =>
                            {
                                options.Host = context.Configuration["host"];
                                options.Token = context.Configuration["token"];
                            })
                            .AddSingleton<IConsoleService, ConsoleService>()
                            .AddSingleton<IConfigService, ConfigService>();
                    });
                })
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
    }
}