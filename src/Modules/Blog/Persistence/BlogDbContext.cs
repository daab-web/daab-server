using Daab.Modules.Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Daab.Modules.Blog.Persistence;

public class BlogDbContext(DbContextOptions<BlogDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts { get; init; }
    public DbSet<Comment> Comments { get; init; }
    public DbSet<CommentTranslation> CommentTranslations { get; init; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite drops DateTimeKind; everything is stored as UTC, so read it back as UTC.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcConverter>();
    }

    private sealed class UtcConverter()
        : ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var posts = modelBuilder.Entity<Post>();
        var comments = modelBuilder.Entity<Comment>();
        var translations = modelBuilder.Entity<CommentTranslation>();

        posts.HasKey(p => p.Id);
        posts.HasIndex(p => p.Slug).IsUnique();
        posts.HasIndex(p => p.AuthorId);
        posts.HasIndex(p => new { p.Status, p.PublishedAt });
        posts.Property(p => p.Status).HasConversion<string>();
        posts.Property(p => p.Title).HasMaxLength(200);
        posts.Property(p => p.Excerpt).HasMaxLength(500);
        posts.Property(p => p.Locale).HasMaxLength(10);

        comments.HasKey(c => c.Id);
        comments.HasIndex(c => new { c.PostId, c.CreatedAt });
        comments.Property(c => c.Content).HasMaxLength(2000);
        comments.Property(c => c.Language).HasMaxLength(10);
        comments
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        translations.HasKey(t => new { t.CommentId, t.Locale });
        translations
            .HasOne(t => t.Comment)
            .WithMany(c => c.Translations)
            .HasForeignKey(t => t.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
