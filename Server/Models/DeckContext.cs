using Microsoft.EntityFrameworkCore;

namespace Casino.Server.Models
{
    public class DeckContext : DbContext
    {
        public DeckContext(DbContextOptions<DeckContext> options)
            : base(options)
        {
        }

        public DbSet<Deck> Decks { get; set; }
    }
}
