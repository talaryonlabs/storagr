using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Storagr.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configuration =>
                {
                    configuration
                        .AddJsonFile("appsettings.json", true)
                        .AddJsonFile("/usr/storagr/config/storagr.ui.json", true)
                        .AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot("/usr/storagr/wwwroot");
                    webBuilder.UseStartup<Startup>();
                });
    }
}