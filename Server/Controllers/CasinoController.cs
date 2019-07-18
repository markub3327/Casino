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

            Console.WriteLine("Pocet hracov pri stole: {0}", context.Players.Count());

            if (context.Players.Count() == 0)
            {
                var croupier = new Models.Croupier
                {
                    Id = 1,
                    Name = "Croupier",
                    Wallet = 10000
                };
                context.Players.Add(croupier);

                // Kazdy hrac ma na zaciatku 2 karty
                context.Cards.Add(new Models.Card
                {
                    PlayerId = croupier.Id,
                    Suit = Models.Card.ESuit.Diamonds,
                    Value = "A"
                });
                context.Cards.Add(new Models.Card
                {
                    PlayerId = croupier.Id,
                    Suit = Models.Card.ESuit.Clubs,
                    Value = "3"
                });

                context.SaveChanges();
            }
        }

        // GET casino/
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            // Vytvor pole hracov obsahujuce aj ich karty
            var players = await _context.Players.Include(p => p.Cards).ToListAsync();

            // Vytvor vyslednu formu JSON pre hraca (obsahuje iba jeho Meno, stav penazenky, karty v hre)
            /*var response = (Models.Player)players.Select(p => new
            {
                p.Name,
                p.Wallet,
                Cards = (List<Models.Card>)p.Cards.Select(c => new { c.Color, c.Suit, c.Value })
            });*/

            return Ok(players);
        }
            
        // GET casino/3
        /*[HttpGet("{id}")]
        public async Task<ActionResult<Models.Player>> Get(long id)
        {
            var todoItem = await _context.Player.FindAsync((id+1));

            if (todoItem == null)
            {
                return NotFound();
            }


            return null;
        }*/

        // POST api/values
        /*[HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
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
