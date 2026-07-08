using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Models;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Translation.Tests.Memory;

public sealed class EmbeddingConverterTests
{
    [Fact]
    public void Embedding_RoundTripsThroughSqliteBlobConverter()
    {
        using var db = new TranslationDb();
        db.Seed(
            TranslationMemoryEntry.Create(
                "s",
                "t",
                ContentType.Scientist,
                BlockType.Paragraph,
                "en",
                "az",
                [1.5f, -2.25f, 3f]
            )
        );

        var reloaded = db.Context.Entries.Single();
        Assert.Equal<float[]?>([1.5f, -2.25f, 3f], reloaded.Embedding);
    }

    [Fact]
    public void Embedding_NullEmbedding_PersistsAsNull()
    {
        using var db = new TranslationDb();
        db.Seed(
            TranslationMemoryEntry.Create(
                "s",
                "t",
                ContentType.Scientist,
                BlockType.Paragraph,
                "en",
                "az",
                null
            )
        );

        Assert.Null(db.Context.Entries.Single().Embedding);
    }
}
