using System;
using System.Threading.Tasks;

namespace Casino.Games
{
    public class Blackjack
    {
        public static readonly string Name = "Blackjack";

        public static readonly string Description = "...";

        public Blackjack()
        {
        }

        // Telo hry
        public Task Run()
        {
            Console.WriteLine("===============================================");
            Program.ShowError("                   Blackjack                   ");
            Console.WriteLine("\n===============================================");

            return Task.Run(() =>
            {
                Program.PlayersList();
            });
        }
    }
}
