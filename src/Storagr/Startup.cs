using System;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Storagr.Data.Entities;
using Storagr.Shared;


namespace Storagr
{
    public class Startup
    {
        private readonly StoragrSettings _storagrSettings;
        
        public Startup(IConfiguration configuration)
        {
            _storagrSettings = new StoragrSettings(configuration);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var hasher = new PasswordHasher<UserEntity>();
            var password = hasher.HashPassword(null, "_storagr");
            
            services.AddStoragrCore(_storagrSettings);
            services.AddStoragrCache(_storagrSettings.CacheSettings);
            services.AddStoragrBackend(_storagrSettings.BackendSettings);
            services.AddStoragrStore(_storagrSettings.StoreSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMigrationRunner migrationRunner)
        {
            if (env.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            migrationRunner.MigrateUp();
        }
    }
}