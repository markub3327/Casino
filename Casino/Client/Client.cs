using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace Casino.Client
{
    public class Client
    {
        // Trieda weboveho klienta
        private readonly HttpClient clientService = new HttpClient();

        // Info o serveri
        private ServerInfo serverInfo;

        // Stav pripojenie
        public bool IsConnected { get; private set; }

        public Client()
        {
            // HttpClient
            clientService.DefaultRequestHeaders.Accept.Clear();
            clientService.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.IsConnected = false;
        }

        public Client(ServerInfo info)
        {
            // HttpClient
            clientService.DefaultRequestHeaders.Accept.Clear();
            clientService.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // uloz adresu
            this.serverInfo = info;

            // Over server
            if (!TryConnection())
            {
                this.serverInfo = null;
                this.IsConnected = false;
            }
            else
            {
                this.IsConnected = true;
            }
        }

        public bool TryConnection()
        {
            if (GetPlayersAsync().Result == null)
                return false;

            Casino.Program.ShowWarning("Connection to server was successful");
            Console.WriteLine("\n");

            return true;
        }

        public void SetServer(ServerInfo info)
        {
            this.serverInfo = info;

            // Vyskusaj sa spojit so serverom
            TryConnection();
        }

        public async Task<List<Items.Player>> GetPlayersAsync()
        {
            // Pokusi sa nacitat zoznam hracov
            try
            {

                // Posli HTTP GET na server
                using (var stream = await clientService.GetStreamAsync(serverInfo.serverUri))
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        using (JsonReader reader = new JsonTextReader(streamReader))
                        {
                            // JSON serializer
                            JsonSerializer serializer = new JsonSerializer();

                            // Nacitaj JSON spravu do zoznamu objektov
                            // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
                            var players = serializer.Deserialize<List<Items.Player>>(reader);

                            // Vypis hracov
                            //foreach (var player in players)
                            //{
                            //    player.Print();
                            //}

                            return players;
                        }
                    }
                }
            }
            catch (Exception e) /*HttpRequestException*/
            {
                Casino.Program.ShowError("\nError with connection!!!\n\tMessage:");
                Console.WriteLine(" {0}", e.Message);

                return null;
            }
        }

        public async Task<Items.Player> GetPlayerAsync(long id)
        {
            var uri = serverInfo.Append(id.ToString());

            // Posli HTTP GET na server
            var str = await clientService.GetStringAsync(uri);

            // Nacitaj JSON spravu do objektu
            // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
            var player = JsonConvert.DeserializeObject<Items.Player>(str);

            return player;
        }

        public async Task<Items.Player> AddPlayerAsync(Items.Player player)
        {
            // Nacitaj objekt do JSON spravy
            // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
            var json = JsonConvert.SerializeObject(player);

            // Obsah spravy
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Posli HTTP POST na server a zachyt odpoved
            var response = await clientService.PostAsync(serverInfo.serverUri, content);

            // Zapis prebehol uspesne
            if (response.IsSuccessStatusCode)
            {
                var newPlayer = JsonConvert.DeserializeObject<Items.Player>(await response.Content.ReadAsStringAsync());

                // Vypis hraca
                newPlayer.Print();

                return newPlayer;
            }

            return null;            
        }
    }
}
