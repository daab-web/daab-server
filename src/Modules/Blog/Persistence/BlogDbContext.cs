using Daab.Modules.Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Daab.Modules.Blog.Persistence;

public class BlogDbContext(DbContextOptions<BlogDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts { get; init; }
    public DbSet<PostTranslation> PostTranslations { get; init; }
    public DbSet<Comment> Comments { get; init; }
    public DbSet<CommentText> CommentTexts { get; init; }

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
        var postTranslations = modelBuilder.Entity<PostTranslation>();
        var comments = modelBuilder.Entity<Comment>();
        var commentTexts = modelBuilder.Entity<CommentText>();

        posts.HasKey(p => p.Id);
        posts.HasIndex(p => p.Slug).IsUnique();
        posts.HasIndex(p => p.AuthorId);
        posts.HasIndex(p => new { p.Status, p.PublishedAt });
        posts.Property(p => p.Status).HasConversion<string>();
        posts.Property(p => p.SourceLocale).HasMaxLength(10);

        postTranslations.HasKey(t => new { t.PostId, t.Locale });
        postTranslations.Property(t => t.Locale).HasMaxLength(10);
        postTranslations.Property(t => t.Title).HasMaxLength(200);
        postTranslations.Property(t => t.Excerpt).HasMaxLength(500);
        postTranslations
            .HasOne(t => t.Post)
            .WithMany(p => p.Translations)
            .HasForeignKey(t => t.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        comments.HasKey(c => c.Id);
        comments.HasIndex(c => new { c.PostId, c.CreatedAt });
        comments.Property(c => c.SourceLocale).HasMaxLength(10);
        comments
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        commentTexts.HasKey(t => new { t.CommentId, t.Locale });
        commentTexts.Property(t => t.Locale).HasMaxLength(10);
        commentTexts.Property(t => t.Content).HasMaxLength(2000);
        commentTexts
            .HasOne(t => t.Comment)
            .WithMany(c => c.Texts)
            .HasForeignKey(t => t.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
