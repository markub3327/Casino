using System;
using Microsoft.EntityFrameworkCore;

namespace Casino.Server.Models
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {
        }

        // Tabulka databazy ukladajuca objekty hracov
        public DbSet<Casino.Models.Player> Players { get; set; }

        // Tabulka databazy ukladajuca karty v hre
        public DbSet<Casino.Models.Card> Cards { get; set; }

        // Tabulka databazy ukladajuca herne balicky kariet
        public DbSet<Deck> Decks { get; set; }
    }
}
