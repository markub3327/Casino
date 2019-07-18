using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Casino.Server.Controllers
{
    [Route("casino/decks")]
    [ApiController]
    public class DecksController : ControllerBase
    {
        private readonly Models.DeckContext _context;

        public DecksController(Models.DeckContext context)
        {
            _context = context;

            Console.WriteLine("Pocet balickov kariet: {0}", _context.Decks.Count());

            if (_context.Decks.Count() == 0)
            {
                // Create a new TodoItem if collection is empty,
                var deck = new Models.Deck();                
                _context.Decks.Add(deck);
                _context.SaveChanges();
            }
        }

        // GET casino/decks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Deck>>> Get()
        {
            var listDecks = await _context.Decks.ToListAsync();

            return listDecks;
        }

        // GET casino/decks/3
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Deck>> Get(long id)
        {
            var deck = await _context.Decks.FindAsync(id);

            if (deck == null)
            {
                return NotFound();
            }

            return deck;
        }
    }
}
