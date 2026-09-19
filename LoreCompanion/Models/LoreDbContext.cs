using System.IO;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.Models
{
    public class LoreDbContext : DbContext
    {
        public DbSet<Item> Items { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Sets up SQLite with the file path of your choice
            string dbFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LoreCompanion");
            Directory.CreateDirectory(dbFolder);
            string dbPath = Path.Combine(dbFolder, "lorecompanion.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EF Core automatically configures 'Id' as the primary key by convention.
            // You can add additional configurations here if needed:
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });
        }

    }
}