using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Storagr;

namespace Storagr.Store
{
    public class Startup
    {
        private readonly StoragrConfig _config;

        public Startup(IConfiguration configuration)
        {
            _config = new StoragrConfig("StoragrStore", configuration);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) => services
            .AddStoreCore(_config)
            .AddStoreServices(_config);

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
        }
    }
}