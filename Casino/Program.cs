using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace Casino
{
    public class Program
    {
        // Referencia na lokalneho hraca, s ktorym hram hru
        private static Items.Player myPlayer;
        
        private static Client.Client client;

        // Hlavne menu
        private static Menu.ListMenu mainMenu;

        public static void Main(string[] args)
        {
            // Encoding
            Console.InputEncoding = System.Text.Encoding.Default;
            Console.OutputEncoding = System.Text.Encoding.Default;

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

        // Hlavne menu hry
        private static void CreateMainMenu()
        {
            mainMenu = new Menu.ListMenu
            {
                Items = new List<Menu.MenuItem>
                {
                    new Menu.MenuItem { Text = "New player", IsEnabled = false, Action = NewPlayer },
                    new Menu.MenuItem { Key = ConsoleKey.F2, Text = "Connect to server", IsEnabled = true, Action = Connect },
                    new Menu.MenuItem { Text = "Create new server", IsEnabled = true },
                    new Menu.MenuItem { Text = "Show profile", IsEnabled = false, Action = Profile },
                    new Menu.MenuItem { Key = ConsoleKey.F1, Text = "About", IsEnabled = true, Action = About },
                    new Menu.MenuItem { Key = ConsoleKey.Escape, Text = "Exit", IsEnabled = true, Action = () => Exit(0)  }
                }
            };
        }

        // Registracia noveho hraca
        private static void NewPlayer()
        {
            string name, betS;
            long bet;

            // Zadajte udaje o novom hracovi
            do {
                // Zadajte meno svojho hraca
                Console.Write("Enter player's name: ");
                name = Console.ReadLine();

                Console.Write("Enter first bet: ");
                betS = Console.ReadLine();
            } while (name == string.Empty || betS == string.Empty || !long.TryParse(betS, out bet));

            Console.WriteLine();

            // Pridaj hraca k stolu
            myPlayer = client.AddPlayerAsync(new Items.Player
            {
                Bet = bet,
                Name = name,
                Wallet = 10000
            }).Result;

            // Ak sa podarilo pridat hraca k stolu povol zobrazenie jeho profilu
            if (myPlayer != null)
            {
                mainMenu.Items[3].IsEnabled = true;
                mainMenu.Items[0].IsEnabled = false;
            }
            else
            {
                mainMenu.Items[3].IsEnabled = false;
            }
        }

        // Pripoj sa na server
        private static void Connect()
        {
            // Vytvor spojenie
            client = new Client.Client(new Client.ServerInfo("localhost", "5000", "casino"));

            // Ak sa pripojil povol vytvorenie hraca
            if (client.IsConnected)
            {
                mainMenu.Items[0].IsEnabled = true;
                mainMenu.Items[2].IsEnabled = false;
            }
            else
            {
                mainMenu.Items[0].IsEnabled = false;
                mainMenu.Items[2].IsEnabled = true;
            }
        }

        // Profil hraca
        private static void Profile()
        {
            myPlayer = client.GetPlayerAsync(myPlayer.Id - 1).Result;         // POCITAME V Db od 1 a v C# od 0 !!!
            myPlayer.Print();
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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("Digital AI Casino");
            Console.ResetColor();

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
