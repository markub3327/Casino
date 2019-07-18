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

            services.AddControllers();
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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Vytvor krupiera do hry
            //var context = app.ApplicationServices.GetService<Models.ApiContext>();
            //AddCroupier(context);
        }

        private static void AddCroupier(Models.ApiContext context)
        {
            var croupier = new Models.Croupier
            {
                Name = "Croupier",
                Wallet = 10000
            };
            context.Players.Add(croupier);

            // Kazdy hrac ma na zaciatku 2 karty
            context.Cards.Add(new Models.Card
            {
                PlayerId = croupier.Id,
                Suit = Models.Card.ESuit.Diamonds,
                Value = "A"
            });
            context.Cards.Add(new Models.Card
            {
                PlayerId = croupier.Id,
                Suit = Models.Card.ESuit.Clubs,
                Value = "3"
            });

            context.SaveChanges();
        }
    }
}
