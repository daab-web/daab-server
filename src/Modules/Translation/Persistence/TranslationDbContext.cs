using Daab.Modules.Translation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Daab.Modules.Translation.Persistence;

public class TranslationDbContext(DbContextOptions<TranslationDbContext> options)
    : DbContext(options)
{
    public DbSet<TranslationMemoryEntry> Entries { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entry = modelBuilder.Entity<TranslationMemoryEntry>();

        entry.HasKey(x => x.Id);
        entry.Property(x => x.ContentType).HasConversion<string>();
        entry.Property(x => x.BlockType).HasConversion<string>();

        // Base-typed locals select the non-generic HasConversion overload, sidestepping the
        // float[] vs float[]? annotation mismatch (EF only invokes the converter for non-null values).
        ValueConverter converter = new ValueConverter<float[], byte[]>(
            vector => EmbeddingBlob.Encode(vector),
            bytes => EmbeddingBlob.Decode(bytes)
        );

        ValueComparer comparer = new ValueComparer<float[]>(
            (left, right) => left.SequenceEqual(right),
            vector => vector.Aggregate(0, (hash, value) => HashCode.Combine(hash, value)),
            vector => vector.ToArray()
        );

        entry.Property(x => x.Embedding).HasConversion(converter, comparer);

        entry
            .HasIndex(x => new
            {
                x.SourceHash,
                x.ContentType,
                x.BlockType,
                x.SourceLocale,
                x.TargetLocale,
            })
            .HasDatabaseName("IX_TM_ExactLookup");
    }
}
