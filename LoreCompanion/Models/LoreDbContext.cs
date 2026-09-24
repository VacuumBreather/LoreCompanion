using LoreCompanion.Views.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LoreCompanion.Models
{
    public class LoreDbContext(DbContextOptions<LoreDbContext> options) : DbContext(options)
    {
        public DbSet<Episode> Episodes => Set<Episode>();

        public DbSet<Location> Locations => Set<Location>();

        public DbSet<Item> Items => Set<Item>();

        public DbSet<DatabaseRelease> DatabaseReleases => Set<DatabaseRelease>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            configurationBuilder.Properties<TimeSpan>().HaveConversion<TimeSpanToSecondsConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DatabaseRelease>()
                        .Property(x => x.Version)
                        .HasConversion(v => v.ToString(), v => Version.Parse(v));

            modelBuilder.Entity<Episode>()
                        .ToTable(t => t.HasCheckConstraint("CK_Episodes_Number_Min", "\"Number\" >= 1"));

            modelBuilder.Entity<Episode>().HasIndex(x => x.Number).IsUnique();
            modelBuilder.Entity<Episode>().HasIndex(x => x.VideoKey).IsUnique();

            modelBuilder.Entity<Location>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Location>().Property(x => x.Name).UseCollation(DatabaseHelper.NoCaseCollation);
            modelBuilder.Entity<Location>().HasOne(x => x.Episode).WithMany().OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Character>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Character>().Property(x => x.Name).UseCollation(DatabaseHelper.NoCaseCollation);
            modelBuilder.Entity<Character>().HasOne(x => x.Location).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Character>().HasOne(x => x.Episode).WithMany().OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Item>().Property(x => x.Name).UseCollation(DatabaseHelper.NoCaseCollation);
            modelBuilder.Entity<Item>().HasOne(x => x.Location).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Item>().HasOne(x => x.Episode).WithMany().OnDelete(DeleteBehavior.Restrict);
        }
    }
}