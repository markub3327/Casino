using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Casino.Server.Models
{
    public class Deck
    {
        // Sukromny generator nahodnych cisiel pre losovanie kariet
        private readonly Random randGen = new Random((int)System.DateTime.Now.Ticks);

        // Jedinecne cislo balicku
        [Key]
        public long Id { get; set; }
        // ID krupiera, ktoremu patri tento balicek kariet
        public string CroupierId { get; set; }
        // Zasobnik kariet reprezentujuci balicek
        public IList<Items.Card> Cards { get; set; }
        

        // Vygeneruj karty do balicku
        public void Create(ApiContext context)
        {
            for (int j = 0; j < 4; j++)
            {
                context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = string.Empty, Value = "A", Suit = (Items.Card.ESuit)j });

                for (int i = 2; i <= 10; i++)
                {
                    context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = string.Empty, Value = i.ToString(), Suit = (Items.Card.ESuit)j });
                }

                context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = string.Empty, Value = "J", Suit = (Items.Card.ESuit)j });
                context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = string.Empty, Value = "Q", Suit = (Items.Card.ESuit)j });
                context.Cards.Add(new Items.Card { DeckId = this.Id, PlayerId = string.Empty, Value = "K", Suit = (Items.Card.ESuit)j });
            }
        }

        // Vezme nahodnu kartu z balicka
        public async Task GetCard(string playerId, ApiContext context)
        {
            var CardsNotUsed = await context.Cards.Where(c => (c.PlayerId == string.Empty)).ToListAsync();
            var cardId = randGen.Next(0, CardsNotUsed.Count);
            var card = CardsNotUsed[cardId];

            // Prirad karte ID hraca
            card.PlayerId = playerId;

            Console.WriteLine("Volne karty v balicku {0}: {1}", this.Id, CardsNotUsed.Count);
        }
    }
}
