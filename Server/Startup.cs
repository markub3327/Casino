using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Casino.Server
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
            // Databaza v RAM pameti
            services.AddDbContext<Models.ApiContext>(opt =>
                            opt.UseInMemoryDatabase("CasinoDb"));

            services.AddControllers()
                .AddNewtonsoftJson();   // JSON.NET use in older version of ASP.NETCore                
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();

            // Add the endpoint routing matcher middleware to the request pipeline
            app.UseRouting();

            // Add the authorization middleware to the request pipeline
            app.UseAuthorization();

            // Add endpoints to the request pipeline
            app.UseEndpoints(endpoints =>
            {
                // Default root URL  (http://localhost:5000)
                endpoints.MapControllerRoute("default", "/casino", new { controller = "Casino", action = "Index" });
                endpoints.MapControllerRoute("default_styles", "/casino/styles.css", new { controller = "Casino", action = "Styles" });
            });
        }        
    }
}
