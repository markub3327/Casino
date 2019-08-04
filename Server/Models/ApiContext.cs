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
        public DbSet<Items.Player> Players { get; set; }

        // Tabulka databazy ukladajuca karty v hre
        public DbSet<Items.Card> Cards { get; set; }

        // Tabulka databazy ukladajuca herne balicky kariet
        public DbSet<Deck> Decks { get; set; }

        // Tabulka databazy obsahujuca ponuku hier 
        public DbSet<Items.Game> Games { get; set; }

        // Tabulka databazy ukladajuca objekty krupierov
        public DbSet<Croupier> Croupiers { get; set; }

        // Tabulka databazy obsahujuca ponuku akcii hraca
        public DbSet<Items.Action> Actions { get; set; }
    }
}
