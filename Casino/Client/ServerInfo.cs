using System;

namespace Casino.Client
{
    public class ServerInfo : Uri
    {
        public ServerInfo() : base(Connect())
        {
        }

        public ServerInfo(string uri) : base(uri)
        {
        }

        public ServerInfo(string serverName, string serverPort, string serverAPI, bool secure) :
            base((secure == true) ? $"https://{serverName}:{serverPort}/{serverAPI}" : $"http://{serverName}:{serverPort}/{serverAPI}")
        {
        }

        // Pripoj k serveru
        public static string Connect()
        {
            string serverName;
            string serverPort;
            string serverAPI;
            bool secure;

            Console.WriteLine("Connect to existing server");
            Console.WriteLine("===============================================\n");

            // Zadajte parametre spojenia
            do
            {
                Console.Write("Enter server name: ");
                serverName = Console.ReadLine();
            } while (serverName == string.Empty);

            do
            {
                Console.Write("Enter server port: ");
                serverPort = Console.ReadLine();
            } while (serverPort == string.Empty);

            do
            {
                Console.Write("Enter api name: ");
                serverAPI = Console.ReadLine();
            } while (serverAPI == string.Empty);

            Console.Write("Use SSL/TLS secure connection (Y/n): ");
            if (Console.ReadKey().Key == ConsoleKey.Y) secure = true;
            else secure = false;

            Console.WriteLine("\n");

            // Vygeneruje adresu pre komunikaciu so serverom
            if (secure)
                return $"https://{serverName}:{serverPort}/{serverAPI}";

            return $"http://{serverName}:{serverPort}/{serverAPI}";
        }

        public ServerInfo Append(string str)
        {
            return new ServerInfo($"{this.AbsoluteUri}/{str}");
        }        
    }
}