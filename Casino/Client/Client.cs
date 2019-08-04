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
    public class Client : HttpClient
    {
        public Client()
        {
            // Media typ definovany v hlavicke
            this.DefaultRequestHeaders.Accept.Clear();
            this.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }        

        // Stav pripojenia
        public bool TryConnection(ServerInfo info)
        {
            var msg = this.GetAsync(info).Result;
            if (msg.IsSuccessStatusCode)
            {
                Program.ShowWarning("Connection to server was successful.");
                Console.WriteLine("\n");
                return true;
            }
            return false;
        }

        // Stiahni zoznam hracov
        public async Task<List<Items.Player>> GetPlayersAsync(ServerInfo info)
        {
            try
            {
                // Posli HTTP GET na server                
                using (var msg = await this.GetAsync(info))
                {
                    if (msg.IsSuccessStatusCode)
                    {
                        using (StreamReader streamReader = new StreamReader(await msg.Content.ReadAsStreamAsync()))
                        {
                            using (JsonReader reader = new JsonTextReader(streamReader))
                            {
                                // JSON serializer
                                JsonSerializer serializer = new JsonSerializer();

                                // Nacitaj JSON spravu do zoznamu objektov
                                // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
                                return serializer.Deserialize<List<Items.Player>>(reader);
                            }
                        }
                    }
                    return null;
                }
            }
            catch (Exception e) //HttpRequestException
            {
                Program.ShowError("\nError with connection!!!\n\tMessage:");
                Console.WriteLine(" {0}", e.Message);
                return null;
            }
        }

        // Stiahni profil hraca
        public async Task<Items.Player> GetPlayerAsync(ServerInfo info)
        {
            try
            {
                // Posli HTTP GET na server
                using (var msg = await this.GetAsync(info))
                {
                    if (msg.IsSuccessStatusCode)
                    {
                        // Nacitaj JSON spravu do objektu
                        // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
                        var newPlayer = JsonConvert.DeserializeObject<Items.Player>(await msg.Content.ReadAsStringAsync());

                        return newPlayer;
                    }
                    return null;
                }
            }
            catch (Exception e) //HttpRequestException
            {
                Program.ShowError("\nError with connection!!!\n\tMessage:");
                Console.WriteLine(" {0}", e.Message);
                return null;
            }
        }

        // Pridaj hraca
        public async Task<Items.Player> AddPlayerAsync(ServerInfo info, Items.Player player)
        {
            // Nacitaj objekt do JSON spravy
            // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
            var json = JsonConvert.SerializeObject(player);

            // Obsah spravy
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Posli HTTP POST na server a zachyt odpoved
            using (var response = await this.PostAsync(info, content))
            {
                // Zapis prebehol uspesne
                if (response.IsSuccessStatusCode)
                {
                    var newPlayer = JsonConvert.DeserializeObject<Items.Player>(await response.Content.ReadAsStringAsync());

                    return newPlayer;
                }
            }
            return null;
        }

        // Nacitaj zoznam hier
        public async Task<List<Items.Game>> GetGamesAsync(ServerInfo info)
        {
            try
            {
                // Posli HTTP GET na server                
                using (var msg = await this.GetAsync(info))
                {
                    if (msg.IsSuccessStatusCode)
                    {
                        using (StreamReader streamReader = new StreamReader(await msg.Content.ReadAsStreamAsync()))
                        {
                            using (JsonReader reader = new JsonTextReader(streamReader))
                            {
                                // JSON serializer
                                JsonSerializer serializer = new JsonSerializer();

                                // Nacitaj JSON spravu do zoznamu objektov
                                // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
                                return serializer.Deserialize<List<Items.Game>>(reader);
                            }
                        }
                    }
                    return null;
                }
            }
            catch (Exception e) //HttpRequestException
            {
                Program.ShowError("\nError with connection!!!\n\tMessage:");
                Console.WriteLine(" {0}", e.Message);
                return null;
            }
        }

        // Nacitaj zoznam akcii
        public async Task<List<Items.Action>> GetActionsAsync(ServerInfo info)
        {
            try
            {
                // Posli HTTP GET na server                
                using (var msg = await this.GetAsync(info))
                {
                    if (msg.IsSuccessStatusCode)
                    {
                        using (StreamReader streamReader = new StreamReader(await msg.Content.ReadAsStreamAsync()))
                        {
                            using (JsonReader reader = new JsonTextReader(streamReader))
                            {
                                // JSON serializer
                                JsonSerializer serializer = new JsonSerializer();

                                // Nacitaj JSON spravu do zoznamu objektov
                                // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
                                return serializer.Deserialize<List<Items.Action>>(reader);
                            }
                        }
                    }
                    return null;
                }
            }
            catch (Exception e) //HttpRequestException
            {
                Program.ShowError("\nError with connection!!!\n\tMessage:");
                Console.WriteLine(" {0}", e.Message);
                return null;
            }
        }

        public async Task<bool> UpdatePlayer(ServerInfo info, Items.Player player)
        {
            // Nacitaj objekt do JSON spravy
            // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
            var json = JsonConvert.SerializeObject(player);

            // Obsah spravy
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Posli HTTP POST na server a zachyt odpoved
            using (var response = await this.PutAsync(info, content))
            {
                // Zapis prebehol uspesne
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
