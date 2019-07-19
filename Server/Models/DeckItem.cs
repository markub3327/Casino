using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Casino.Server.Models
{
    public class Deck
    {
        // Sukromny generator nahodnych cisiel pre losovanie kariet
        private readonly Random randGen;
        // Jedinecne cislo balicku
        public long Id { get; set; }
        // Zasobnik kariet reprezentujuci balicek
        public List<Casino.Models.Card> Cards { get; set; }
        
        // Konstruktor balicku vytvori karty
        public Deck(long Id, ApiContext context)
        {
            this.Id = Id;
            this.randGen = new Random((int)System.DateTime.Now.Ticks);

            for (int j = 0; j < 4; j++)
            {
                context.Cards.Add(new Casino.Models.Card { DeckId = this.Id, Value = "A", Suit = (Casino.Models.Card.ESuit)j });

                for (int i = 2; i <= 10; i++)
                {
                    context.Cards.Add(new Casino.Models.Card { DeckId = this.Id, Value = i.ToString(), Suit = (Casino.Models.Card.ESuit)j });
                }

                context.Cards.Add(new Casino.Models.Card { DeckId = this.Id, Value = "J", Suit = (Casino.Models.Card.ESuit)j });
                context.Cards.Add(new Casino.Models.Card { DeckId = this.Id, Value = "Q", Suit = (Casino.Models.Card.ESuit)j });
                context.Cards.Add(new Casino.Models.Card { DeckId = this.Id, Value = "K", Suit = (Casino.Models.Card.ESuit)j });
            }
        }

        // Vezme nahodnu kartu z balicka
        public void GetCard(long playerId, ApiContext context)
        {
            var CardsNotUsed = context.Cards.Where(c => (c.PlayerId == 0)).ToList();
            var cardId = randGen.Next(0, CardsNotUsed.Count);
            var card = CardsNotUsed[cardId];

            card.PlayerId = playerId;

            Console.WriteLine("Volne karty v balicku {0}: {1}", this.Id, CardsNotUsed.Count());
        }

        // Vezme nahodnu kartu z balicka
        public async System.Threading.Tasks.Task GetCardAsync(long playerId, ApiContext context)
        {
            var CardsNotUsed = await context.Cards.Where(c => (c.PlayerId == 0)).ToListAsync();
            var cardId = randGen.Next(0, CardsNotUsed.Count);
            var card = CardsNotUsed[cardId];

            card.PlayerId = playerId;

            Console.WriteLine("Volne karty v balicku {0}: {1}", this.Id, CardsNotUsed.Count());
        }
    }
}
