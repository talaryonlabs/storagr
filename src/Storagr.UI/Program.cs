using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Storagr.UI
{
    public class Program
    {
        public static void Main(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configuration =>
            {
                configuration
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile("/usr/storagr/config/storagr.ui.json", true)
                    .AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                // if(Directory.Exists("/usr/storagr/wwwroot"))
                //     webBuilder.UseContentRoot("/usr/storagr/wwwroot");
                // else if (Directory.Exists("./wwwroot"))
                //     webBuilder.UseContentRoot("./wwwroot");
                //
                // var test = new DirectoryInfo(".");
                //
                webBuilder.UseStartup<Startup>();
            })
            .Build()
            .Run();
    }
}