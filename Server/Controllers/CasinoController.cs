using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Casino.Server.Controllers
{
    public class CasinoController : ControllerBase
    {
        private readonly Models.ApiContext _context;

        public CasinoController(Models.ApiContext context)
        {
            _context = context;

            Console.WriteLine("Main Casino's page");
            Console.WriteLine($"Pocet hracov na serveri: {_context.Players.Count()}");
        }

        // GET casino/
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Casino.Server.index.html"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return new ContentResult
                    {
                        ContentType = "text/html",
                        StatusCode = StatusCodes.Status200OK,
                        Content = await reader.ReadToEndAsync()
                    };
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Styles()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Casino.Server.styles.css"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return new ContentResult
                    {
                        ContentType = "text/css",
                        StatusCode = StatusCodes.Status200OK,
                        Content = await reader.ReadToEndAsync()
                    };
                }
            }
        }

        // GET casino/players
        // GET casino/players?token=XXX
        [HttpGet("{controller}/players"), Produces("application/json")]
        public async Task<IActionResult> GetPlayers([FromQuery(Name = "token")]string token)
        {
            Console.WriteLine($"token = {token}");

            if (!string.IsNullOrEmpty(token))
            {
                // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
                var players = await _context.Players.Where(p =>
                    (p.Token == token)).Include(p => p.Cards).ToListAsync();

                // Ak existuje hrac (s tokenom)
                if (players.Count > 0)
                {
                    return Ok(players[0]);
                }
                return NotFound();
            }
            else
            {
                // Vytvor pole hracov na serveri
                var players = await _context.Players.ToListAsync();

                // Vytvor vystup JSON zo zoznamu hracov pri stole
                return Ok(players.Select(p => new
                {
                    name = p.Name,
                    gameid = p.GameId
                }));
            }
        }

        // POST casino/players
        [HttpPost("{controller}/players"), Produces("application/json")]
        public async Task<IActionResult> PostPlayer([FromBody]Items.Player player)
        {
            // Nesmie byt objekt hraca prazdny
            if (player != null)
            {
                // Over ci uz hrac s rovnakym menom neexistuje
                if (!await _context.Players.ContainsAsync(player))
                {
                    // Vygeneruj bezpecnostny token pre komunikaciu
                    TokenGenerator.Generate(player);

                    // Pridaj hraca do Db
                    await _context.Players.AddAsync(player);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetPlayers), new { token = player.Token }, player);
                }
            }

            return BadRequest();
        }

        // PUT casino/players
        /*[HttpPut("{controller}/players"), Produces("application/json")]
        public async Task<IActionResult> PutPlayer([FromQuery]string token, [FromBody]Items.Player player)
        {
            // Nesmie byt objekt hraca prazdny
            if (player != null)
            {
                // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
                var players = await _context.Players.Where(p => (p.Token == token)).ToListAsync();

                // Ak existuje v Db hrac (s tokenom)
                if (players.Count > 0)
                {
                    // Aktualizuj vyber hracovej hry
                    players[0].GameId = player.GameId;

                    // Uloz zmeny profilu hraca do Db
                    await _context.SaveChangesAsync();

                    return NoContent();
                }
            }

            return BadRequest();
        }*/
    }
}
