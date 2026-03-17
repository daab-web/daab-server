using Daab.Modules.Activities.Models;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Persistence;

public class ActivitiesDbContext(DbContextOptions<ActivitiesDbContext> options) : DbContext(options)
{
    internal DbSet<News> News { get; init; }
    internal DbSet<Attachment> Attachments { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var news = modelBuilder.Entity<News>();
        var attachments = modelBuilder.Entity<Attachment>();

        news.HasKey(n => n.Id);
        news.HasIndex(n => n.Title, "news_title_idx");
        news.HasIndex(n => n.Slug).IsUnique();

        attachments
            .HasOne<News>()
            .WithMany(n => n.Attachments)
            .HasForeignKey(a => a.ParentObjectId);
    }
}
