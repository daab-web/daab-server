using Daab.Modules.Scientists.Models;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Persistence;

public class ScientistsDbContext : DbContext
{
    public DbSet<Scientist> Scientists { get; set; }
    public DbSet<Application> Applications { get; set; }

    public ScientistsDbContext(DbContextOptions<ScientistsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var application = modelBuilder.Entity<Application>();

        application.HasKey(s => s.Id);
    }
}
