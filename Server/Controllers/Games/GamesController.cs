using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Casino.Server.Controllers
{
    public class GamesController : ControllerBase
    {
        private readonly Models.ApiContext _context;

        public GamesController(Models.ApiContext context)
        {
            _context = context;

            // Ak v Db nie su hry vytvor zaznamy
            if (_context.Games.Count() == 0)
            {
                // Pridaj hru do Db
                _context.Games.Add(new Items.Game { Name = "Blackjack", Description = "Card game" });
                _context.Games.Add(new Items.Game { Name = "Poker", Description = "Card game" });
                _context.SaveChanges();
            }
        }

        // GET casino/games
        // GET casino/game?name=XXX
        [HttpGet("casino/{controller}"), Produces("application/json")]
        public async System.Threading.Tasks.Task<IActionResult> GetGamesAsync([FromQuery(Name = "name")]string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                var games = await _context.Games.ToListAsync();

                return Ok(games.Select(g => new
                {
                    name = g.Name
                }));
            }
            else
            {
                var games = await _context.Games.Where(g => (g.Name == name)).ToListAsync();

                if (games.Count > 0)
                    return Ok(games[0]);

                return NotFound();
            }
        }
    }
}
