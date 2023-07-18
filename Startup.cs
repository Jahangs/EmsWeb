using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;


namespace EmsWeb
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public static string _data = ""; // Always start application with OnGrid mode
        public static string EmsLogs = "";
        //---------------------------------------------------------------------
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
           //Configuration = configuration;

           Configuration = new ConfigurationBuilder()
           .AddJsonFile("EmsConfigs.json")
           .Build();            
        }
        //---------------------------------------------------------------------
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }
        //---------------------------------------------------------------------
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            
            app.UseDefaultFiles();
            
            app.UseRouting();

	        app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });



            Startup._data = Configuration["EmsConfigs"];

            //string[] _splited_data = Startup._data.Split(",");
            //Startup.OnGrid = _splited_data[0];
            //Startup.OffGrid = _splited_data[1];
            //Startup.StartConverter = _splited_data[2];
            //Startup.StopConverter = _splited_data[3];
        }
        //---------------------------------------------------------------------
    }
}
