using System;
using System.Collections.Generic;

namespace Casino.Server.Models
{
    public class Deck
    {
        // Jedinecne cislo balicku
        public long Id { get; set; }
        // Zasobnik kariet reprezentujuci balicek
        public List<Card> Cards { get; set; }
        
        // Konstruktor balicku vytvori karty
        public Deck()
        {
            this.Cards = new Stack<Card>(52);

            for (int j = 0; j < 4; j++)
            {
                this.Cards.Push(new Card { Value = "A", Suit = (Card.ESuit)j });

                for (int i = 2; i <= 10; i++)
                {
                    this.Cards.Push(new Card { Value = i.ToString(), Suit = (Card.ESuit)j });
                }

                this.Cards.Push(new Card { Value = "J", Suit = (Card.ESuit)j });
                this.Cards.Push(new Card { Value = "Q", Suit = (Card.ESuit)j });
                this.Cards.Push(new Card { Value = "K", Suit = (Card.ESuit)j });
            }
        }
        // Vezme kartu zvrchu balicka
        public Card GetCard()
        {
            return Cards.Pop();
        }
    }
}
