using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Linq;

namespace Casino
{
    public class Program
    {
        // Trieda weboveho klienta
        private static readonly HttpClient clientService = new HttpClient();
        // Informacie o servery
        private static string serverUri;

        public static async Task Main(string[] args)
        {
            // HttpClient
            clientService.DefaultRequestHeaders.Accept.Clear();
            clientService.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Encoding
            Console.InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // Starting game
            Start();

            do {
                // Hlavne menu hry
                await MainMenu();
            } while (true);
        }

        private static void Start()
        {
            Console.Clear();
            Console.Write("Welcome to ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("Digital AI Casino");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Martin Horvath \u00A9 Copyright 2019 MIT");
            Console.WriteLine("Website: https://github.com/marhor3327/Casino.git");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("University of Ss. Cyril and Methodius in Trnava");
            Console.ResetColor();
            Console.WriteLine("===============================================\n");
        }

        private static async Task MainMenu()
        {            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("Digital AI Casino");
            Console.ResetColor();
            Console.WriteLine("===============================================");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("Menu");
            Console.ResetColor();
            Console.WriteLine("===============================================");
            Console.ResetColor();
            Console.WriteLine("F1 - Connect to server");
            Console.WriteLine("Esc - Exit\n");

            // Cakaj na stlacenie klavesy
            var key = Console.ReadKey(true);

            // Vykonanie prikazu
            switch (key.Key)
            {
                case ConsoleKey.F1:
                    await Connect();
                    break;
                case ConsoleKey.Escape:
                    // Koniec hry
                    Console.WriteLine("Thank you for playing");
                    Console.WriteLine("Bay bay \u263A");
                    Environment.Exit(0);
                    break;     
                default:
                    // Zly prikaz
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine("You entered bad command. Please try again.\n");
                    Console.ResetColor();
                    Console.Beep();
                    break;               
            }

            //Console.Clear();
        }

        private static async Task Connect()
        {
            string serverName;
            string serverPort;
            bool secure;

            Console.WriteLine("Connect to existing server");
            Console.WriteLine("===============================================\n");

            do {
                Console.Write("Enter server name: ");
                serverName = Console.ReadLine();
                Console.Write("Enter server port: ");
                serverPort = Console.ReadLine();
                Console.Write("Use SSL/TLS secure connection (Y/n): ");
                if (Console.ReadKey().Key == ConsoleKey.Y) secure = true;
                else                                       secure = false;
            } while (serverName == string.Empty || serverPort == string.Empty);

            // Vyskusaj sa spojit so serverom
            try	
            {
                // Vygeneruje adresu pre komunikaciu so serverom
                if (secure)
                    serverUri = string.Format("https://{0}:{1}/casino/", serverName, serverPort);
                else
                    serverUri = string.Format("http://{0}:{1}/casino/", serverName, serverPort);

                var msg = await clientService.GetStringAsync(serverUri);
                Console.WriteLine(msg);
            }  
            catch(Exception e) /*HttpRequestException*/
            {
                ShowError("\nError!!!\n\tMessage:");
                Console.WriteLine(" {0}", e.Message);
            }
        }

        private static void ShowError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(text);
            Console.ResetColor();
        }

        private static void ShowWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(text);
            Console.ResetColor();
        }        
    }
}
