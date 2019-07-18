﻿using System;
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
        public int Wallet { get; set; }
        // Karty na ruke
        public List<Card> Cards { get; set; }
    }
}