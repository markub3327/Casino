using System;

namespace Casino.Server.Models
{
    public class Card
    {
        public enum ESuit : byte
        {
            Hearts = 0,     // Srdcova karta
            Clubs = 1,      // Krizova karta (trojlistok)
            Diamonds = 2,   // Kosostvorec
            Spades = 3      // Pikova karta
        }   

        public enum EColor : byte
        {
            Black = 0,      // Cierna
            Red = 1         // Cervena
        }

        // Jedinecne cislo karty
        public long Id { get; set; }
        // ID hraca, ktoremu patri tato karta
        public long PlayerId { get; set; }
        // ID balicku, do ktoreho patri tato karta
        public long DeckId { get; set; }
        // Farba karty
        public EColor  Color
        {
            get {
                if (this.Suit == ESuit.Hearts || this.Suit == ESuit.Diamonds)
                    return EColor.Red;
                else
                    return EColor.Black;
            }            
        }
        // Symbol karty
        public ESuit   Suit  { get; set; }
        // Hodnota karty
        public string  Value { get; set; }
    }
}
