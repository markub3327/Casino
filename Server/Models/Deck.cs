//
// Deck.cs
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Casino.Server.Models
{
    public class Deck
    {
        // Jedinecne cislo balicku
        [Key]
        public long Id { get; set; }
        // ID krupiera, ktoremu patri tento balicek kariet
        public string CroupierId { get; set; }
        // Zasobnik kariet reprezentujuci balicek
        public IList<Items.Card> Cards { get; set; }

        // Konstruktor prazdneho balicku
        public Deck() { }

        // Vygeneruj karty do balicku
        public Deck(ApiContext context)
        {
            for (int j = 0; j < 4; j++)
            {
                context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = null, Value = "A", Suit = (Items.Card.ESuit)j });

                for (int i = 2; i <= 10; i++)
                {
                    context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = null, Value = i.ToString(), Suit = (Items.Card.ESuit)j });
                }

                context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = null, Value = "J", Suit = (Items.Card.ESuit)j });
                context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = null, Value = "Q", Suit = (Items.Card.ESuit)j });
                context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = null, Value = "K", Suit = (Items.Card.ESuit)j });
            }
        }
    }
}
