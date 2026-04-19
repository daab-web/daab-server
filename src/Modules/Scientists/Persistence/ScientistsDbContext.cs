using Daab.Modules.Scientists.Models;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Persistence;

public class ScientistsDbContext : DbContext
{
    public DbSet<Scientist> Scientists { get; set; }
    public DbSet<ScientistTranslation> ScientistTranslations { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Publication> Publications { get; set; }
    public DbSet<Director> Directors { get; set; }

    public ScientistsDbContext(DbContextOptions<ScientistsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var application = modelBuilder.Entity<Application>();
        var scientistTranslations = modelBuilder.Entity<ScientistTranslation>();

        application.HasKey(s => s.Id);

        modelBuilder.Entity<Director>().HasIndex(d => d.ScientistId).IsUnique();

        scientistTranslations.HasKey(nt => new { nt.ScientistId, nt.Locale });
        scientistTranslations.HasIndex(nt => new { nt.ScientistId, nt.Locale }).IsUnique();
        scientistTranslations
            .HasOne(st => st.Scientist)
            .WithMany(s => s.Translations)
            .HasForeignKey(st => st.ScientistId)
            .OnDelete(DeleteBehavior.Cascade);
        scientistTranslations.Property(nt => nt.Status).HasConversion<string>();
    }
}
