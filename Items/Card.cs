//
// Card.cs
//
// Author:
//       Martin Horvath <marhor3327@gmail.com>
//
// Copyright (c) 2019 2019
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.ComponentModel.DataAnnotations;

namespace Casino.Items
{
    public class Card
    {
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


        public enum ESuit
        {
            Hearts,         // Srdcova karta
            Clubs,          // Krizova karta (trojlistok)
            Diamonds,       // Kosostvorec
            Spades          // Pikova karta
        }

        public enum EColor
        {
            Black,          // Cierna
            Red             // Cervena
        }
    }
}
