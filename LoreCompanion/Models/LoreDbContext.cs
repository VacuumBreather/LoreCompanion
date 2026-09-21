using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.Models
{
    public class LoreDbContext(DbContextOptions<LoreDbContext> options) : DbContext(options)
    {
        public DbSet<Item> Items => Set<Item>();

        public DbSet<DatabaseRelease> DatabaseReleases => Set<DatabaseRelease>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DatabaseRelease>()
                        .Property(x => x.Version)
                        .HasConversion(v => v.ToString(), v => Version.Parse(v));
        }
    }
}