using Daab.Modules.Activities.Models;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Persistence;

public class ActivitiesDbContext(DbContextOptions<ActivitiesDbContext> options) : DbContext(options)
{
    public DbSet<News> News { get; init; }
    public DbSet<NewsTranslation> NewsTranslations { get; init; }
    public DbSet<Attachment> Attachments { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var news = modelBuilder.Entity<News>();
        var newsTranslations = modelBuilder.Entity<NewsTranslation>();
        var attachments = modelBuilder.Entity<Attachment>();

        news.HasKey(n => n.Id);
        news.HasIndex(n => n.Title, "news_title_idx");
        news.HasIndex(n => n.Slug).IsUnique();

        newsTranslations.HasKey(nt => new { nt.NewsId, nt.Locale });
        newsTranslations.HasIndex(nt => new { nt.NewsId, nt.Locale }).IsUnique();
        newsTranslations
            .HasOne(nt => nt.News)
            .WithMany(n => n.Translations)
            .HasForeignKey(nt => nt.NewsId)
            .OnDelete(DeleteBehavior.Cascade);
        newsTranslations.Property(nt => nt.Status).HasConversion<string>();

        attachments
            .HasOne<News>()
            .WithMany(n => n.Attachments)
            .HasForeignKey(a => a.ParentObjectId);
    }
}
