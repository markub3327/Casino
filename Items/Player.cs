using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Casino.Items
{
    public class Player
    {
        // Meno hraca (prezyvka)
        [Key]
        public string Nickname { get; set; }
        // Penazeka
        public long Wallet { get; set; }
        // Vyska stavky hraca
        public long Bet { get; set; }
        // Karty na ruke
        //public IEnumerable<Card> Cards { get; set; }
        // Hracova hra
        public string GameId { get; set; }
        // Token hraca
        public string Token { get; set; }
        // Akcia hraca
        public EActions Action { get; set; }
        // Stav hraca
        public EState State { get; set; }
    }

    public enum EActions
    {
        NONE,
        HIT,
        STAND,
        DOUBLE,
        EXIT
    }
    
    public enum EState
    {
        FREE,
        PLAYING,
        WIN,
        LOSE,
        DRAW
    }
}
