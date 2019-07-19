using System;
using System.Collections.Generic;

namespace Casino.Server.Models
{
    public class Player
    {
        // Jedinecne cislo hraca
        public long Id { get; set; }
        // Meno hraca
        public string Name { get; set; }
        // Penazeka
        public long Wallet { get; set; }
        // Vyska stavky hraca, minimalna stavka je 100$
        public long Bet { get; set; }
        // Karty na ruke
        public List<Card> Cards { get; set; }
    }
}
