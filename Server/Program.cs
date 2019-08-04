using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Casino.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Run(args);
        }

        public static void Run(string[] args)
        {
            // Uvodna sprava
            Head();

            // Novy server
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void Head()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Welcome to AI Casino Server");
            Console.ResetColor();
            Console.WriteLine("Martin Horvath\tCopyright 2019");
            Console.WriteLine("University of Ss. Cyril and Methodius in Trnava");
            Console.WriteLine("===============================================");
            Console.WriteLine("Connect to https://{0}:5001/casino\n", GetIP());
        }

        private static string GetIP()
        {
            return /*Dns.GetHostAddresses(*/Dns.GetHostName()/*).ToString()*/;
        }
    }
}
