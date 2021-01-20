using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
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
                .UseTypoCorrections()
                .UseHost(hostBuilder =>
                {
                    hostBuilder.ConfigureAppConfiguration(configuration =>
                    {
                        configuration
                            .AddJsonFile("appsettings.json", true)
                            .AddConfigFile()
                            .AddTokenFile();
                    });
                    hostBuilder.ConfigureServices((context, services) =>
                    {
                        services
                            .AddStoragrClient(options =>
                            {
                                context.Properties.TryGetValue(typeof(InvocationContext), out var invocationContext);
                                var parseResult = (invocationContext as InvocationContext)?.ParseResult.RootCommandResult;
                                
                                options.Host = parseResult?.OptionResult("--host")?.GetValueOrDefault<string>() ??
                                               context.Configuration["host"];
                                options.Token = parseResult?.OptionResult("--token")?.GetValueOrDefault<string>() ??
                                                context.Configuration["token"];
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