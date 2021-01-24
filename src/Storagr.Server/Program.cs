using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Storagr.Server
{
    public static class Program
    {
        public static void Main(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.AddStoragr();
            })
            .ConfigureAppConfiguration(configuration =>
            {
                configuration
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile("/usr/storagr/config/storagr.json", true)
                    .AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
            .Build()
            .Run();
    }
}