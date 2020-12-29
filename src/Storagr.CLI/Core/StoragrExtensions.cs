using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Storagr.Client;

namespace Storagr.CLI
{
    public static class StoragrExtensions
    {
        public static IConsoleService GetConsole(this IHost host)
        {
            return host.Services.GetRequiredService<IConsoleService>();
        }

        public static IStoragrClient GetStoragrClient(this IHost host)
        {
            return host.Services.GetRequiredService<IStoragrClient>();
        }

        public static IConfigurationBuilder AddTokenFile(this IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .AddJsonFile(StoragrConstants.TokenFilePath, true);

            return configurationBuilder;
        }

        public static IConfigurationBuilder AddConfigFile(this IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .AddJsonFile("/usr/storagr/config/storagr.cli.json", true);
            
            return configurationBuilder;
        }
    }
}