using System;

namespace Casino.Client
{
    public class ServerInfo
    {
        // Adresa servera
        public Uri serverUri { get; private set; }

        public ServerInfo()
        {

        }

        public ServerInfo(string api)
        {
            Connect(api);
        }


        public ServerInfo(string serverName, string serverPort, string serverAPI)
        {
            this.serverUri = new Uri($"http://{serverName}:{serverPort}/{serverAPI}");
        }


        // Pripoj k serveru
        public void Connect(string api)
        {
            string serverName;
            string serverPort;
            bool secure;

            Console.WriteLine("Connect to existing server");
            Console.WriteLine("===============================================\n");

            // Zadajte parametre spojenia
            do
            {
                //Console.Write("Enter server name: ");
                serverName = "localhost";//Console.ReadLine();

                //Console.Write("Enter server port: ");
                serverPort = "5000";//Console.ReadLine();

                Console.Write("Use SSL/TLS secure connection (Y/n): ");
                if (Console.ReadKey().Key == ConsoleKey.Y) secure = true;
                else secure = false;

                Console.WriteLine("\n");
            } while (serverName == string.Empty || serverPort == string.Empty);

            // Vygeneruje adresu pre komunikaciu so serverom
            if (secure)
                this.serverUri = new Uri($"http://{serverName}:{serverPort}/{api}");
            else
                this.serverUri = new Uri($"http://{serverName}:{serverPort}/{api}");
        }

        public string Append(string str)
        {
            return ($"{serverUri.OriginalString}/{str}");
        }
    }
}
