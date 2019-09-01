//
// BlackjackController.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Casino.Server.Controllers
{
    [ApiController]
    public class BlackjackController : ControllerBase
    {
        // Referencia na Db
        private readonly Models.ApiContext context;

        public BlackjackController(ILogger<BlackjackController> logger, Models.ApiContext context)
        {
            this.context = context;

            if (context.Croupiers.Where(p => p.Nickname == "Croupier-Blackjack").Count() < 1)
            {
                var croupier = new Models.Croupier
                {
                    Nickname = "Croupier-Blackjack",
                    Wallet = 1000000,   // krupier ma bank (kasino zacina s 1.000.000$/krupier existenciu)
                    GameId = "Blackjack",
                    State = Items.Player.EState.PLAYING
                };
                context.Croupiers.Add(croupier);

                var deck = new Models.Deck(context) { CroupierId = croupier.Nickname };
                context.Decks.Add(deck);
                context.SaveChanges();

                // Krupier zacina s jednou kartou hru
                RandomCard(croupier).Wait();

                context.SaveChanges();
            }
        }

        // GET http://localhost:5000/casino/players/
        [HttpGet("casino/[controller]/players")]
        public async Task<IActionResult> GetPlayers()
        {
            // Vytvor pole hracov na serveri
            var players = await context.Players.Where(p => p.GameId == "Blackjack")
                .Include(p => p.Cards)
                .ToListAsync();

            var croupierDb = players.Where(p => p.Nickname == "Croupier-Blackjack")
                .ToArray();

            // Ak uz vsetci hraci ukoncili hru na rade je krupier
            if ((croupierDb[0].State != Items.Player.EState.PLAYING) &&
                !players.Any(p => p.State != Items.Player.EState.PLAYING && p.Nickname != "Croupier-Blackjack"))
            {
                // Vynuluj krupierov profil
                croupierDb[0].Action = Items.Player.EActions.NONE;
                croupierDb[0].State = Items.Player.EState.PLAYING;

                // Uvolni krupierove karty
                foreach (var card in croupierDb[0].Cards)
                    card.PlayerId = null;

                // Krupier zacina s jednou kartou hru
                await RandomCard(croupierDb[0]);

                // Uloz do Db
                await context.SaveChangesAsync();
            }
            // Ak este hraci hraju postaraj sa o krupierove karty
            else
            {
                var standPlayers = players.Where(p => p.Action == Items.Player.EActions.STAND);
                if (standPlayers.Count() == (players.Count() - 1))
                {
                    await CroupierGame(croupierDb[0]);
                }
            }

            // Vytvor vystup JSON zo zoznamu hracov pri stole
            return Ok(players.Select(p => new
            {
                p.Nickname,
                p.Bet,
                Cards = p.Cards.Select(c => new { c.Suit, c.Value }),
                p.State,
                p.Action,
                p.Wallet
            }));
        }

        // PUT http://localhost:5000/casino/players/
        [HttpPut("casino/[controller]/get-card")]
        public async Task<IActionResult> GetCard([FromBody]Items.Player player)        
        {
            if (player != null)
            {
                // Autorizacia hraca pomocou generovaneho tokenu
                var playerDb = await context.Players.Where(p => p.Nickname == player.Nickname && p.Token == player.Token)
                    .Include(p => p.Cards)
                    .ToArrayAsync();

                // Ak existuje
                if (playerDb[0] != null)
                {
                    await RandomCard(playerDb[0]);

                    // Prida zaznam do Db
                    await context.SaveChangesAsync();

                    // Bez obsahu v odpovedi
                    return NoContent();
                }

                return NotFound();
            }


            // Zla poziadavka
            return BadRequest();
        }

        // DELETE http://localhost:5000/casino/players/
        [HttpDelete("casino/[controller]/free-card")]
        public async Task<IActionResult> FreeCard([FromBody]Items.Player player)
        {
            if (player != null)
            {
                // Autorizacia hraca pomocou generovaneho tokenu
                var playerDb = await context.Players.Where(p => p.Nickname == player.Nickname && p.Token == player.Token)
                    .Include(p => p.Cards)
                    .ToArrayAsync();

                // Ak existuje
                if (playerDb[0] != null)
                {
                    foreach (var card in playerDb[0].Cards)
                        card.PlayerId = null;

                    // Prida zaznam do Db
                    await context.SaveChangesAsync();

                    // Bez obsahu v odpovedi
                    return NoContent();
                }

                return NotFound();
            }

            // Zla poziadavka
            return BadRequest();
        }

        // Vyber nahodnu kartu
        private async Task RandomCard(Items.Player player)
        {
            // Nahodny vyber volnej karty z balicku
            var randGen = new Random((int)DateTime.Now.Ticks);
            var CardsNotUsed = await context.Cards.Where(c => (c.PlayerId == null)).ToArrayAsync();

            if (CardsNotUsed.Length > 0)
            {
                var cardId = randGen.Next(0, CardsNotUsed.Length);

                // Prirad karte ID hraca
                CardsNotUsed[cardId].PlayerId = player.Nickname;

                Console.WriteLine($"Zostalo {CardsNotUsed.Length} volnych kariet");
            }
            else
            {
                Console.WriteLine("No cards in deck.");
            }

            Console.WriteLine($"Player cards = {player.CardSum}");
        }

        private async Task CroupierGame(Items.Player croupier)
        {
            // Hra krupiera (asynchronna uloha)
            await Task.Run(async () =>
            {
                // Krupier si taha karty do suctu 17 a viac
                do
                {
                    // Tahaj karty dokym nemas aspon 17 
                    var Csum = croupier.CardSum;
                    if (Csum >= 17)
                    {
                        // Krupier ukoncil hru idu sa vyplacat vyhry
                        croupier.Action = Items.Player.EActions.STAND;

                        // Prida zaznam do Db
                        await context.SaveChangesAsync();

                        // Koniec
                        break;
                    }

                    // Prirad nahodnu kartu
                    await RandomCard(croupier);

                    // Krupier ukoncil hru idu sa vyplacat vyhry
                    croupier.Action = Items.Player.EActions.HIT;

                    // Uloz do Db
                    await context.SaveChangesAsync();
                } while (true);
            });
        }
    }
}

