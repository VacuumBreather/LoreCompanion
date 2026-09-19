using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.Models
{
    public class LoreDbContext(DbContextOptions<LoreDbContext> options) : DbContext(options)
    {
        public DbSet<Item> Items => Set<Item>();
    }
}