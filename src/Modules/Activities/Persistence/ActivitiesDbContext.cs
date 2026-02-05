using Microsoft.EntityFrameworkCore;

using Daab.Modules.Activities.Models;

namespace Daab.Modules.Activities.Persistence;

public class ActivitiesDbContext(DbContextOptions<ActivitiesDbContext> options) : DbContext(options)
{
    internal DbSet<News> News { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var news = modelBuilder.Entity<News>();

        news.HasIndex(n => n.Title, "news_title_idx");
    }
}