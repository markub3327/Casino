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
    }
}
