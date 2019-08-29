using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ConsoleTables;

namespace Casino
{
    public class Program
    {
        // Referencia na vlastneho hraca
        public static Items.Player myPlayer { get; private set; }

        // Http cesta k api
        public static Uri serverUri { get; private set; }

        // Hlavne menu hry
        private static Menu.ListMenu mainMenu;

        // Referencia na hranu hru
        private static Games.Blackjack game;

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

            do
            {
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
                    new Menu.MenuItem { Text = "New player", IsEnabled = false, Action = NewPlayer },
                    new Menu.MenuItem { Key = ConsoleKey.F5, Text = "Create new server", IsEnabled = true, Action = NewServer },
                    new Menu.MenuItem { Text = "Players list", IsEnabled = false, Action = PlayersList },
                    new Menu.MenuItem { Text = "Profile", IsEnabled = false, Action = Profile },
                    new Menu.MenuItem { Key = ConsoleKey.F1, Text = "About", IsEnabled = true, Action = About },
                    new Menu.MenuItem { Key = ConsoleKey.Escape, Text = "Exit", IsEnabled = true, Action = () => Exit(0) } // Vypnute korektne cez menu code 0
                }
            };
        }

        // Novy server
        private static void NewServer()
        {
            Server.Program.Main(null);
        }

        // Pripoj sa na server
        private static void Connect()
        {
            string serverName;

            do {
                do
                {
                    Console.Write("Hostname: ");
                    serverName = Console.ReadLine();
                } while (string.IsNullOrEmpty(serverName));

                serverUri = new Uri($"http://{serverName}:5000/casino/"); //= new Client.ServerInfo();

                if (TryConnection())
                {
                    ShowWarning("Connection was successful.");
                    Console.WriteLine("\n");
                    break;
                }

                ShowError("Cannot connect to server.");
            } while (true);

            mainMenu.Items[2].IsEnabled = true;  // novy hrac
            mainMenu.Items[3].IsEnabled = false;  // novy server
            mainMenu.Items[4].IsEnabled = true;  // zoznam hracov na serveri
        }

        private static bool TryConnection()
        {


            return true;
        }

        // Registracia noveho hraca
        private static void NewPlayer()
        {
            string name;

            // Zadajte udaje o novom hracovi
            do
            {
                // Zadajte meno svojho hraca
                Console.Write("Enter your player's nickname: ");
                name = Console.ReadLine();
            } while (name == string.Empty);

            Console.WriteLine();

            // Vytvor spojenie
            using (var client = new Client())
            {
                // Vytvor objekt hraca
                myPlayer = (Items.Player) client.AddItemAsync(new Uri(serverUri, "players/create"), new Items.Player { Nickname = name }).Result;

                // Ak sa podarilo pridat hraca do kasina povol zobrazenie jeho profilu a novu hru
                if (myPlayer != null)
                {                    
                    mainMenu.Items[1].IsEnabled =  true;     // nova hra
                    mainMenu.Items[2].IsEnabled = false;     // novy hrac
                    mainMenu.Items[5].IsEnabled =  true;     // profil hraca

                    ShowWarning("Adding player was successful.");
                    Console.WriteLine("\n");
                }
                else
                {
                    ShowError($"Player '{name}' cannot be created.");
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
            using (var client = new Client())
            {
                    // Menu hier
                    var gameMenu = new Menu.ListMenu("Games")
                    {
                        Items = new List<Menu.MenuItem>()
                    };

                    gameMenu.AddItem(new Menu.MenuItem { Text = Games.Blackjack.Name, IsEnabled = true, Action = () =>
                    // Spusti hru blackjack
                    {
                        game = new Games.Blackjack();

                        game.Run();                         
                    }});
                    

                    // Vrati sa na hlavne menu
                    gameMenu.AddItem(new Menu.MenuItem { Text = "Back", IsEnabled = true, Action = () => { return; } });

                    gameMenu.InvokeResult().Wait();                
            }
        }

        // Zobraz profil hraca  
        public static void Profile()
        {
            // Vytvor spojenie
            using (var client = new Client())
            {
                myPlayer = client.GetPlayerAsync(new Uri(serverUri, "players/player"), myPlayer).Result;
                var table = new ConsoleTable("Nickname", "Game", "Wallet [$]", "State");
                if (myPlayer != null)
                {
                    table.AddRow(myPlayer.Nickname, myPlayer.GameId, myPlayer.Wallet, myPlayer.State);
                    table.Configure(o => { o.NumberAlignment = Alignment.Right; o.EnableCount = false; });
                    table.Write(Format.Alternative);
                }
            }
        }

        // Zoznam hracov
        public static void PlayersList()
        {
            using (var client = new Client())
            {
                var players = (List<Items.Player>) client.GetListAsync(new Uri(serverUri, "players/all"), typeof(List<Items.Player>)).Result;
                var table = new ConsoleTable("Nickname", "Game");

                foreach (var p in players)
                {
                    table.AddRow(p.Nickname, p.GameId);
                }
                table.Configure(o => o.EnableCount = false);
                table.Write(Format.Alternative);                
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