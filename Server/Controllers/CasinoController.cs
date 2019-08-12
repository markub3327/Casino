using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Casino.Server.Controllers
{
    [ApiController]
    public class CasinoController : ControllerBase
    {
        private readonly Models.ApiContext context;

        public CasinoController(ILogger<CasinoController> logger, Models.ApiContext context)
        {
            this.context = context;
        }

        // GET http://localhost:5000/casino/players/all
        [HttpGet("[controller]/players/all")]
        public async Task<IActionResult> GetPlayers()
        {
            // Vytvor pole hracov na serveri
            var players = await context.Players.Select(p => new
            {
                p.Nickname,
                p.GameId
            }).ToListAsync();

            // Vytvor vystup JSON zo zoznamu hracov pri stole
            return Ok(players);
        }

        // GET http://localhost:5000/casino/players/player
        [HttpGet("[controller]/players/player")]
        public async Task<IActionResult> GetPlayer([FromBody]Items.Player player)
        {
            if (player != null)
            {
                var playerDb = await context.Players.FindAsync(player.Nickname);

                // Autorizacia hraca pomocou generovaneho tokenu
                if (playerDb != null && playerDb.Token == player.Token)
                {
                    return Ok(playerDb);
                }

                return NotFound();
            }

            return BadRequest();
        }

        // POST http://localhost:5000/casino/players/player
        [HttpPost("[controller]/players/create")]
        public async Task<IActionResult> CreatePlayer([FromBody]Items.Player player)
        {
            if (!await context.Players.ContainsAsync(player))
            {
                // Novy hrac nehra este ziadnu hru
                player.GameId = "Nothing";

                // Novy hraci dostanu 10.000$
                if (player.Wallet <= 0)
                    player.Wallet = 10000;

                // Prida zaznam do Db
                context.Players.Add(player);
                await context.SaveChangesAsync();

                // Vytvoreny uspesne, vrati profil
                return CreatedAtAction(nameof(GetPlayer), player);
            }

            // Zla poziadavka
            return BadRequest();
        }
    }
}
