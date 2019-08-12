using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Casino
{
    public class Client : HttpClient
    {
        public async Task<object> AddItemAsync(Uri uri, object item)
        {
            try
            {
                // Nacitaj objekt do JSON spravy
                // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
                var json = System.Text.Json.JsonSerializer.Serialize(item);

                // Obsah spravy
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Posli HTTP POST na server a zachyt odpoved
                using (var response = await this.PostAsync(uri, content))
                {
                    // Zapis prebehol uspesne
                    if (response.IsSuccessStatusCode)
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<Items.Player>(await response.Content.ReadAsStringAsync());
                    }
                }

                return null;
            }
            catch (Exception e) //HttpRequestException
            {
                Program.ShowError("\nError with connection!!!\n\tMessage:");
                Console.WriteLine(" {0}", e.Message);
                return null;
            }
        }

        public async Task<Items.Player> GetPlayerAsync(Uri uri, Items.Player player)
        {
            try
            {
                // Nacitaj objekt do JSON spravy
                var json = System.Text.Json.JsonSerializer.Serialize(player);

                // Telo spravy odosielany na server k overeniu pristupu ku hracovi
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                // Posli HTTP GET na server a zachyt odpoved
                using (var response = await this.SendAsync(request))
                {
                    // Zapis prebehol uspesne
                    if (response.IsSuccessStatusCode)
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<Items.Player>(await response.Content.ReadAsStringAsync());
                    }
                }

                return null;
            }
            catch (Exception e) //HttpRequestException
            {
                Program.ShowError("\nError with connection!!!\n\tMessage:");
                Console.WriteLine(" {0}", e.Message);
                return null;
            }
        }

        public async Task<object> GetListAsync(Uri uri, Type type)
        {
            try
            {
                // Posli HTTP GET na server                
                using (var response = await this.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (StreamReader streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                        {
                            using (Newtonsoft.Json.JsonReader reader = new Newtonsoft.Json.JsonTextReader(streamReader))
                            {
                                // JSON serializer
                                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                                // Nacitaj JSON spravu do zoznamu objektov
                                // JSON size doesn't matter because only a small piece is read at a time from the HTTP request
                                return serializer.Deserialize(reader, type);
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
    }
}
