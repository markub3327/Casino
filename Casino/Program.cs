using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Casino
{
    public class Program
    {
        // Referencia na lokalneho hraca, s ktorym hram hru
        public static Items.Player myPlayer;
        
        // Hlavne menu
        private static Menu.ListMenu mainMenu;

        // Informacie o serveri
        private static Client.ServerInfo baseUri;   // = new Client.ServerInfo("localhost", "5000", "casino", false);

        // Referencia na hranu hru
        private static Games.Game game;

        // Hlavna funkcia main hry
        public static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Encoding
                Console.InputEncoding = System.Text.Encoding.Unicode;
                Console.OutputEncoding = System.Text.Encoding.Unicode;
            }

            // Udalost pri nahlom ukonceni app
            Console.CancelKeyPress += Console_CancelKeyPress;
            
            // Vytvori hlavne menu
            CreateMainMenu();

            // Vycisti obrazovku na zaciatku hry
            Console.Clear();

            do {
                // Hlavicka menu
                Head();

                // Vykonaj zvolenu akciu
                mainMenu.InvokeResult().Wait();
            } while (true);
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Vymaze hraca od aktivnej hry
            if (game != null)
                game.Exit();

            Exit(1);    // Vypnute nasilne code 1
        }

        // Hlavne menu hry
        private static void CreateMainMenu()
        {
            mainMenu = new Menu.ListMenu
            {
                Items = new List<Menu.MenuItem>
                {
                    new Menu.MenuItem { Key = ConsoleKey.F2, Text = "Connect to server", IsEnabled = true, Action = Connect },
                    new Menu.MenuItem { Key = ConsoleKey.F3, Text = "New game", IsEnabled = false, Action = NewGame },
                    new Menu.MenuItem { Key = ConsoleKey.F4, Text = "New player", IsEnabled = false, Action = NewPlayer },
                    new Menu.MenuItem { Key = ConsoleKey.F5, Text = "Create new server", IsEnabled = true, Action = NewServer },
                    new Menu.MenuItem { Text = "Show profile", IsEnabled = false, Action = Profile },
                    new Menu.MenuItem { Key = ConsoleKey.F1, Text = "About", IsEnabled = true, Action = About },
                    new Menu.MenuItem { Key = ConsoleKey.Escape, Text = "Exit", IsEnabled = true, Action = () => Exit(0) } // Vypnute korektne cez menu code 0
                }
            };
        }

        // Novy server
        private static void NewServer()
        {
            Server.Program.Run(null);
        }

        // Pripoj sa na server
        private static void Connect()
        {
            baseUri = new Client.ServerInfo();

            // Vytvor spojenie
            using (var client = new Client.Client())
            {
                // Ak sa pripojil povol vytvorenie hraca
                if (client.TryConnection(baseUri))
                {
                    mainMenu.Items[2].IsEnabled = true; // novy hrac
                    mainMenu.Items[3].IsEnabled = false; // novy server
                }
                else
                {
                    mainMenu.Items[2].IsEnabled = false;
                    mainMenu.Items[3].IsEnabled = true;
                }
            }
        }

        // Registracia noveho hraca
        private static void NewPlayer()
        {
            string name;

            // Zadajte udaje o novom hracovi
            do
            {
                // Zadajte meno svojho hraca
                Console.Write("Enter player's name: ");
                name = Console.ReadLine();
            } while (name == string.Empty);

            Console.WriteLine();

            // Vytvor spojenie
            using (var client = new Client.Client())
            {
                // Vytvor objekt hraca
                myPlayer = client.AddPlayerAsync(baseUri.Append("players"), new Items.Player
                {
                    Name = name,
                    Wallet = 10000,  // Novy hrac dostava 10000$
                    State = Items.Player.EState.None
                }).Result;

                // Ak sa podarilo pridat hraca do kasina povol zobrazenie jeho profilu a novu hru
                if (myPlayer != null)
                {
                    mainMenu.Items[1].IsEnabled = true;     // nova hra
                    mainMenu.Items[2].IsEnabled = false;    // novy hrac
                    mainMenu.Items[4].IsEnabled = true;     // profil hraca

                    ShowWarning("Adding player was successful.");
                    Console.WriteLine("\n");
                }
                else
                {
                    ShowError($"Player with name '{name}' cannot be created.");
                    Console.WriteLine();
                    ShowWarning("Maybe player already exist on server.");
                    Console.WriteLine("\n");
                }
            }            
        }

        // Zacni hrat novu hru
        private static void NewGame()
        {
            // Vytvor spojenie
            using (var client = new Client.Client())
            {
                var games = client.GetGamesAsync(baseUri.Append("games")).Result;
                if (games != null)
                {
                    var gameMenu = new Menu.ListMenu("Game menu")
                    {
                        Items = new List<Menu.MenuItem>()
                    };

                    foreach (var g in games)
                    {
                        gameMenu.AddItem(new Menu.MenuItem { Text = g.Name, IsEnabled = true, Action = () => SelectGame(g.Name) });
                    }

                    gameMenu.InvokeResult().Wait();
                }
            }
        }

        // Akcia podla nazvu hry
        private static void SelectGame(string name)
        {
            if (name == "Blackjack")
            {
                game = new Games.Blackjack(baseUri.Append("blackjack"));

                game.Play();
            }
        }

        // Zobraz profil hraca  
        public static void Profile()
        {
            // Vytvor spojenie
            using (var client = new Client.Client())
            {
                var newPlayer = client.GetPlayerAsync(baseUri.Append($"players?token={Uri.EscapeDataString(myPlayer.Token)}")).Result;
                if (newPlayer != null)
                {
                    myPlayer = newPlayer;
                    myPlayer.Print();
                }
            }
        }

        // Hlavicka
        private static void Head()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("Digital AI Casino");
            Console.ResetColor();
            Console.WriteLine();
        }

        // O hre
        private static void About()
        {
            Console.Clear();

            Console.Write("Welcome to ");
            Head();

            Console.WriteLine("Martin Horvath \u00A9 Copyright 2019 MIT");
            Console.WriteLine("Website: https://github.com/marhor3327/Casino.git");
            Console.WriteLine("University of Ss. Cyril and Methodius in Trnava");
            Console.WriteLine("===============================================\n");
        }

        // Ukoncenie hry
        private static void Exit(int code)
        {
            // Korektny koniec hry
            Console.WriteLine("Thank you for playing");
            Console.WriteLine("Bay bay \u263A");
            Environment.Exit(code);
        }

        // Spravy (chyby, varovania)
        public static void ShowError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(text);
            Console.ResetColor();
        }
        public static void ShowWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(text);
            Console.ResetColor();
        }
    }
}
