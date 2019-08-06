using System;
using System.Linq;
using System.Collections.Generic;

namespace Casino.Games
{
    public class Blackjack
    {
        // Blackjack herne menu
        private static Menu.ListMenu menu;

        // Informacie o serveri
        private Client.ServerInfo info;

        private readonly int MAX_WIDTH = 25;

        public Blackjack(Client.ServerInfo info)
        {
            this.info = info;

            using (var client = new Client.Client())
            {
                // Vytvor menu akcii
                var actions = client.GetActionsAsync(info.Append("actions")).Result;
                if (actions != null)
                {
                    menu = new Menu.ListMenu("Actions menu")
                    {
                        Items = new List<Menu.MenuItem>()
                    };

                    foreach (var a in actions)
                    {
                        menu.AddItem(new Menu.MenuItem { Text = a.Name, IsEnabled = true, Action = () => { Program.myPlayer.ActionId = a.Name; } });
                    }
                }

                // Pridaj hraca ku stolu
                var newPlayer = client.AddPlayerAsync(info.Append("players"), Program.myPlayer).Result;
                if (newPlayer != null)
                {
                    Program.myPlayer = newPlayer;
                }
                else
                {
                    Program.ShowError("Communication error");
                    Console.WriteLine("\n");
                }
            }

            // Hlavicka
            Head();
        }

        // Jadro hry
        public void Play()
        {
            while (true)
            {
                // Hrac zada vysku stavky, s ktorou ide do hry
                Program.myPlayer.Bet = SetBet();

                // Ak hrac vsadi 0 konci hru, alebo ked nema dost penazi na stavky
                if (Program.myPlayer.Wallet < Program.myPlayer.Bet)
                {
                    Program.ShowError("You don't have enaught money for play");
                    Console.WriteLine("\n");
                    break;
                }
                if (Program.myPlayer.Bet <= 0)
                {
                    break;
                }

                // Aktualizuj profil hraca na servery
                UpdatePlayer();
                GetPlayer();

                // Jadro hry
                using (var client = new Client.Client())
                {
                    // Prebieha, kym hrac neskonci a neodide od stolu
                    while (true)
                    {                        
                        if (Program.myPlayer.ActionId == "Stand")
                        {
                            //Console.Clear();
                            Program.ShowWarning("Waiting for round end.");
                            Console.Write("\r");
                        }
                        else
                        {
                            Print();

                            // Hrac ukoncil hru na serveri
                            if (Program.myPlayer.State != Items.Player.EState.Playing)
                            {
                                // Ukoncit hru
                                break;
                            }

                            // Zavolaj menu akcii hry
                            menu.InvokeResult().Wait();

                            // Aktualizuj profil hraca podla vybranej akcie
                            UpdatePlayer();
                        }

                        GetPlayer();
                    }
                }
            }

            // Na konci hry vymaz hraca od stola
            RemovePlayer();
        }

        // Vymaz hraca
        private void RemovePlayer()
        {
            using (var client = new Client.Client())
            {
                client.RemovePlayerAsync(info.Append($"players?token={Uri.EscapeDataString(Program.myPlayer.Token)}")).Wait();
            }
        }

        // Nacitaj vysku stavky z klavesnice
        private long SetBet()
        {
            string betS;
            long bet;

            do {
                Console.Write("Enter bet [$]: ");
                betS = Console.ReadLine();
            } while (!long.TryParse(betS, out bet));
            Console.WriteLine();

            return bet;
        }

        // Aktualizuj profil hraca
        private void UpdatePlayer()
        {
            using (var client = new Client.Client())
            {
                if (!client.UpdatePlayerAsync(info.Append($"players?token={Uri.EscapeDataString(Program.myPlayer.Token)}"), Program.myPlayer).Result)
                {
                    Program.ShowError("Communication error");
                    Console.WriteLine("\n");
                }
            }
        }

        private void GetPlayer()
        {
            Items.Player newPlayer;

            using (var client = new Client.Client())
            {
                if ((newPlayer = client.GetPlayerAsync(info.Append($"players?token={Uri.EscapeDataString(Program.myPlayer.Token)}")).Result) != null)
                {
                    Program.myPlayer = newPlayer;
                }
                else
                {
                    Program.ShowError("Communication error");
                    Console.WriteLine("\n");
                }
            }
        }

        // Vypis stolu na konzolu
        private void Print()
        {
            int /*width,*/ height;

            try
            {
                using (var client = new Client.Client())
                {
                    var players = client.GetPlayersAsync(info.Append("players")).Result;

                    if (players != null && players.Any())
                    {
                        var croupier = players.Where(p => (p.Name == "Croupier-Blackjack")).ToArray()[0];
                        var playersArray = players.Where(p => (p.Name != "Croupier-Blackjack")).ToArray();

                        if (croupier != null)
                        {
                            Console.CursorLeft = /*(i * MAX_WIDTH) +*/((playersArray.Count() * MAX_WIDTH) >> 1) + ((MAX_WIDTH >> 1) - (croupier.Name.Length >> 1));
                            Console.WriteLine($"{croupier.Name}");

                            // Sucet kariet hraca
                            var sumS = croupier.CardSum.ToString();
                            Console.CursorLeft = /*(i * MAX_WIDTH)*/((playersArray.Count() * MAX_WIDTH) >> 1) + ((MAX_WIDTH >> 1) - (sumS.Length >> 1));
                            Console.WriteLine(sumS);

                            Console.CursorLeft = /*(i * MAX_WIDTH) +*/((playersArray.Count() * MAX_WIDTH) >> 1) + ((MAX_WIDTH >> 1) - (croupier.Cards.Count() << 1)) - 1;
                            foreach (var c in croupier.Cards)
                            {
                                c.Print();
                                Console.Write(" ");
                            }
                            Console.Write("\n\n\n\n\n\n\n");
                        }

                        if (playersArray != null && playersArray.Length > 0)
                        {
                            height = 0;
                            for (int i = 0; i < playersArray.Length; i++)
                            {
                                // Karty hraca
                                if (playersArray[i].Cards.Any())
                                {
                                    foreach (var c in playersArray[i].Cards)
                                    {
                                        Console.CursorLeft = (i * MAX_WIDTH) + (MAX_WIDTH >> 1) - 1;
                                        c.Print();
                                        Console.WriteLine();
                                        height++;
                                    }
                                }

                                // Vyska stavky hraca
                                var betS = $"{playersArray[i].Bet}$";
                                Console.CursorLeft = (i * MAX_WIDTH) + ((MAX_WIDTH >> 1) - (betS.Length >> 1));
                                Console.WriteLine(betS);
                                height++;

                                // Ak ide o mojho hraca vypis stav penazenky
                                if (playersArray[i].Name == Program.myPlayer.Name)
                                {
                                    var walletS = $"{Program.myPlayer.Wallet}$";
                                    Console.CursorLeft = (i * MAX_WIDTH) + ((MAX_WIDTH >> 1) - (walletS.Length >> 1));
                                    Console.WriteLine(walletS);
                                    height++;
                                }

                                // Meno hraca
                                Console.CursorLeft = (i * MAX_WIDTH) + ((MAX_WIDTH >> 1) - (playersArray[i].Name.Length >> 1));
                                Console.WriteLine(playersArray[i].Name);
                                height++;

                                // Sucet kariet hraca
                                var sumS = playersArray[i].CardSum.ToString();
                                Console.CursorLeft = (i * MAX_WIDTH) + ((MAX_WIDTH >> 1) - (sumS.Length >> 1));
                                Console.WriteLine(sumS);
                                height++;

                                // Vysledny stav hry
                                var stateS = playersArray[i].State.ToString();
                                Console.CursorLeft = (i * MAX_WIDTH) + ((MAX_WIDTH >> 1) - (stateS.Length >> 1));
                                if (playersArray[i].State == Items.Player.EState.Win)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.Write(stateS);
                                    Console.ResetColor();
                                    Console.WriteLine();
                                }
                                else if (playersArray[i].State == Items.Player.EState.Lose)
                                {
                                    Program.ShowError(stateS);
                                    Console.WriteLine();
                                }
                                else if (playersArray[i].State == Items.Player.EState.Draw)
                                {
                                    Program.ShowWarning(stateS);
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Console.WriteLine(stateS);
                                }
                                height++;

                                if (i < (playersArray.Length - 1))
                                {
                                    Console.CursorTop -= height;
                                    height = 0;
                                }
                            }
                        }

                        Console.WriteLine("\n");
                    }
                }
            }
            catch (Exception e)
            {
                Console.Clear();
                Program.ShowError($"Blackjack table error\n\tMessage: {e.Message}");
                Console.WriteLine();
            }
        }

        private void Head()
        {
            Console.WriteLine("===============================================");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("                    Blackjack                  ");
            Console.ResetColor();
            Console.WriteLine("===============================================\n");
        }
    }
}
