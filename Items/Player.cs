using System;
using System.Collections.Generic;

namespace Casino.Items
{
    public class Player
    {
        // Jedinecne cislo hraca
        public long Id { get; set; }
        // Meno hraca
        public string Name { get; set; }
        // Penazeka
        public long Wallet { get; set; }
        // Vyska stavky hraca
        public long Bet { get; set; }
        // Karty na ruke
        public List<Card> Cards { get; set; }

        // Vypis udajov o hracovi na konzolu
        public void Print()
        {
            Console.WriteLine("Player {0}:", this.Id);
            Console.WriteLine(" - Name: {0}", this.Name);
            Console.WriteLine(" - Wallet: {0}", this.Wallet);
            Console.WriteLine(" - Bet: {0}\n", this.Bet);
            foreach (var card in Cards)
            {
                card.Print();
                Console.Write("   ");
            }

            Console.WriteLine("\n");
        }
    }
}
