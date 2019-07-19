using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Casino.Server.Controllers
{
    [Route("casino/")]
    [ApiController]
    public class CasinoController : ControllerBase
    {
        private readonly Models.ApiContext _context;

        public CasinoController(Models.ApiContext context)
        {
            _context = context;

            Console.WriteLine("Pocet hracov pri stole: {0}", _context.Players.Count());

            // Zisti ci uz je vytvoreny krupier v hre
            if (_context.Players.Count() == 0)
            {
                var deck = new Models.Deck(1, context);
                _context.Decks.Add(deck);
                _context.SaveChanges();

                var croupier = new Models.Croupier
                {
                    Name = "Croupier",
                    Wallet = 10000,
                    Bet = 100
                };
                _context.Players.Add(croupier);
                _context.SaveChanges();

                // Kazdy hrac ma na zaciatku 2 karty
                deck.GetCard(croupier.Id, _context);
                deck.GetCard(croupier.Id, _context);
                _context.SaveChanges();
            }
        }

        // GET casino/
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Vytvor pole hracov obsahujuce aj ich karty
            var players = await _context.Players.Include(p => p.Cards).ToListAsync();

            return Ok(players);
        }
            
        // GET casino/3
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var players = await _context.Players.Include(p => p.Cards).ToListAsync();

            if (id < 0 || id >= players.Count)
            {
                return NotFound();
            }

            return Ok(players[id]);
        }

        // POST casino/
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Casino.Models.Player player)
        {
            // minimalna stavka je 100$
            if (player.Bet < 100) player.Bet = 100;

            await _context.Players.AddAsync(player);
            await _context.SaveChangesAsync();

            Console.WriteLine("player ID {0}", player.Id);
            Console.WriteLine("new player {0}", player.Name);
            Console.WriteLine("player's wallet {0}", player.Wallet);
            Console.WriteLine("player's bet {0}", player.Bet);
             
            // Kazdy hrac ma na zaciatku 2 karty
            var deck = _context.Decks.First();
            await deck.GetCardAsync(player.Id, _context);
            await deck.GetCardAsync(player.Id, _context);
            await _context.SaveChangesAsync();
                
            return CreatedAtAction(nameof(Get), new { id = player.Id }, player);
        }
        
        // PUT api/values/5
        /*[HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
