using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;


namespace Storagr.Server
{
    public class Startup
    {
        private readonly StoragrConfig _config;

        public Startup(IConfiguration configuration)
        {
            _config = new StoragrConfig("Storagr.Server", configuration);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        { 
            services.AddStoragrCore(_config);
            services.AddStoragrSecurity(_config);
            services.AddStoragrCache(_config);
            services.AddStoragrDatabase(_config);
            services.AddStoragrStore(_config);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMigrationRunner migrationRunner, IStoragrLoggerProvider loggerProvider)
        {
            if (env.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            migrationRunner.MigrateUp();
            loggerProvider.Enable();
        }
    }
}