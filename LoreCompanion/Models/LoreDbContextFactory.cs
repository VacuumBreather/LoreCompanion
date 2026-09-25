using LoreCompanion.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LoreCompanion.Models
{
    public class LoreDbContextFactory : IDesignTimeDbContextFactory<LoreDbContext>
    {
        public LoreDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<LoreDbContext>().UseSqlite(AppHelper.ConnectionString).Options;

            return new LoreDbContext(options);
        }
    }
}