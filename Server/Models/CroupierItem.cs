using System;
using System.Collections.Generic;

namespace Casino.Server.Models
{
    public class Croupier : Items.Player
    {
        // Krupier drzi herne balicky kariet
        public IList<Deck> Decks { get; set; }

        // Vypis udajov o krupierovi na konzolu
        public override void Print()
        {
            Console.WriteLine($"Player {Name}:");
            Console.WriteLine($"\tWallet: {Wallet}");
            Console.WriteLine($"\tBet: {Bet}");
            Console.WriteLine($"\tPlaying: {GameId}");
            Console.WriteLine($"\tAction: {ActionId}");
            Console.WriteLine($"\tToken: {Token}");
            if (Decks != null)
            {
                Console.Write("\tDecks:");
                foreach (var deck in Decks)
                {
                    Console.Write($" {deck.Id}");
                }
                Console.WriteLine("\n");
            }
            else
            {
                Console.WriteLine();
            }
            if (Cards != null)
            {
                Console.Write("\t");
                foreach (var card in Cards)
                {
                    card.Print();
                    Console.Write("\t");
                }
                Console.WriteLine("\n");
            }
        }
    }
}
