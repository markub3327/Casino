using System;
using System.Collections.Generic;

namespace Casino.Models
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
    }
}
