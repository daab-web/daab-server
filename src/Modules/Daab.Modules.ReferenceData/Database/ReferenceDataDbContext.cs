using Daab.Modules.ReferenceData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.ReferenceData.Database;

public class ReferenceDataDbContext(DbContextOptions<ReferenceDataDbContext> options)
    : DbContext(options)
{
    public DbSet<Translation> Translations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var translations = modelBuilder.Entity<Translation>();

        translations.HasKey(t => t.Id);
        translations
            .HasIndex(t => new
            {
                t.Locale,
                Ns = t.Namespace,
                t.Key,
            })
            .IsUnique();
    }
}
