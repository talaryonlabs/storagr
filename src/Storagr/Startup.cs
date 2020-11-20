using System;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;


namespace Storagr
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var options = new StoragrSettings()
            {
                TokenSettings =
                {
                  Secret  = Configuration["STORAGR_TOKEN_SECRET"] ?? Configuration["Storagr:Token:Secret"],
                  Issuer  = Configuration["STORAGR_TOKEN_ISSUER"] ?? Configuration["Storagr:Token:Issuer"],
                  Audience  = Configuration["STORAGR_TOKEN_AUDIENCE"] ?? Configuration["Storagr:Token:Audience"],
                  Expiration  = int.Parse(Configuration["STORAGR_TOKEN_EXPIRATION"] ?? Configuration["Storagr:Token:Expiration"]),
                },
                BackendSettings =
                {
                    Type = Enum.Parse<StoragrBackendType>(Configuration["STORAGR_BACKEND_TYPE"] ?? Configuration["Storagr:Backend:Type"], true),
                    DataSource = Configuration["STORAGR_BACKEND_DATASOURCE"] ?? Configuration["Storagr:Backend:DataSource"],
                },
                StoreSettings =
                {
                    Type = Enum.Parse<StoragrStoreType>(Configuration["STORAGR_STORE_TYPE"] ?? Configuration["Storagr:Store:Type"], true),
                    RootPath = Configuration["STORAGR_STORE_ROOTPATH"] ?? Configuration["Storagr:Store:RootPath"],
                },
                CacheSettings =
                {
                    Type = Enum.Parse<StoragrCacheType>( Configuration["STORAGR_CACHE_TYPE"] ?? Configuration["Storagr:Cache:Type"], true),
                    SizeLimit = int.Parse(Configuration["STORAGR_CACHE_SIZELIMIT"] ?? Configuration["Storagr:Cache:SizeLimit"]),
                    Host = Configuration["STORAGR_CACHE_HOST"] ?? Configuration["Storagr:Cache:Host"],
                }
            };
            services.AddStoragrCore(options);
            services.AddStoragrCache(options.CacheSettings);
            services.AddStoragrBackend(options.BackendSettings);
            services.AddStoragrStore(options.StoreSettings);
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
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            migrationRunner.MigrateUp();
        }
    }
}