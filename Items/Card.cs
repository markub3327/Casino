using System;
using System.ComponentModel.DataAnnotations;

namespace Casino.Items
{
    public class Card
    {
        public enum ESuit : byte
        {
            Hearts,         // Srdcova karta
            Clubs,          // Krizova karta (trojlistok)
            Diamonds,       // Kosostvorec
            Spades          // Pikova karta
        }

        public enum EColor : byte
        {
            Black,          // Cierna
            Red             // Cervena
        }

        // Jedinecne cislo karty
        [Key]
        public long Id { get; set; }
        // ID hraca, ktoremu patri tato karta
        public string PlayerId { get; set; }
        // ID balicku, do ktoreho patri tato karta
        public long DeckId { get; set; }
        // Farba karty
        public EColor Color
        {
            get
            {
                if (this.Suit == ESuit.Hearts || this.Suit == ESuit.Diamonds)
                    return EColor.Red;

                return EColor.Black;
            }
        }
        // Symbol karty
        public ESuit Suit { get; set; }
        // Hodnota karty
        public string Value { get; set; }

        // Vypis udajov o karte na konzolu
        public void Print()
        {
            // Nastav farbu konzoly podla karty
            if (this.Color == EColor.Black)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Red;
            }

            // Hodnota karty
            Console.Write(this.Value);

            //  Symbol karty
            switch (this.Suit)
            {
                case ESuit.Hearts:
                    Console.Write("\u2665");
                    break;
                case ESuit.Diamonds:
                    Console.Write("\u2666");
                    break;
                case ESuit.Clubs:
                    Console.Write("\u2663");
                    break;
                case ESuit.Spades:
                    Console.Write("\u2660");
                    break;
            }

            Console.ResetColor();
        }
    }
}