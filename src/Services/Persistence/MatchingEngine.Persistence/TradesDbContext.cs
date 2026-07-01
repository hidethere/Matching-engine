using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchingEngine.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MatchingEngine.Persistence
{
    public class TradesDbContext : DbContext
    {
        public TradesDbContext(DbContextOptions<TradesDbContext> options) : base(options)
        {
        }
        public DbSet<TradeRecord> Trades => Set<TradeRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var t = modelBuilder.Entity<TradeRecord>();
            t.ToTable("Trades");
            t.HasKey(x => x.Id);
            t.Property(x => x.Id).ValueGeneratedNever();
            t.Property(x => x.Symbol).HasMaxLength(20);
            t.HasIndex(x => x.Symbol);

        }
    }
}
