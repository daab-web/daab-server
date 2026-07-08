using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Models;

namespace Daab.Modules.Translation.Tests.Memory;

public sealed class TranslationMemoryTests
{
    static TranslationMemoryEntry Entry(
        string source,
        string target,
        float[]? embedding = null,
        ContentType contentType = ContentType.Scientist,
        BlockType blockType = BlockType.Paragraph,
        string sourceLocale = "en",
        string targetLocale = "az"
    ) =>
        TranslationMemoryEntry.Create(
            source,
            target,
            contentType,
            blockType,
            sourceLocale,
            targetLocale,
            embedding
        );

    static TranslationLookup Lookup(string source) =>
        new(source, ContentType.Scientist, BlockType.Paragraph, "en", "az");

    static TranslationQuery Query(string source, int k = 5) =>
        new(source, ContentType.Scientist, BlockType.Paragraph, "en", "az", K: k);

    [Fact]
    public async Task FindExact_IdenticalSource_ReturnsStoredTargetVerbatim()
    {
        using var db = new TranslationDb();
        db.Seed(Entry("<0>Hello</0>", "<0>Salam</0>"));
        var repo = new TranslationMemory(db.Context, new FakeEmbedder());

        var target = await repo.FindExactAsync(Lookup("<0>Hello</0>"), CancellationToken.None);

        Assert.Equal("<0>Salam</0>", target);
    }

    [Fact]
    public async Task FindExact_MatchesAcrossWhitespaceAndUnicodeForm()
    {
        using var db = new TranslationDb();
        // Code points keep the source pure ASCII: composed U+00C5 vs "A" + combining ring U+030A.
        string composed = ((char)0x00C5) + "  x"; // composed ring, double space
        string decomposed = "A" + (char)0x030A + " x"; // decomposed ring, single space
        db.Seed(Entry(composed, "kofe"));
        var repo = new TranslationMemory(db.Context, new FakeEmbedder());

        var target = await repo.FindExactAsync(Lookup(decomposed), CancellationToken.None);

        Assert.Equal("kofe", target);
    }

    [Fact]
    public async Task FindExact_ChangedSource_ReturnsNull()
    {
        using var db = new TranslationDb();
        db.Seed(Entry("<0>Hello</0>", "<0>Salam</0>"));
        var repo = new TranslationMemory(db.Context, new FakeEmbedder());

        Assert.Null(await repo.FindExactAsync(Lookup("<0>Goodbye</0>"), CancellationToken.None));
    }

    [Fact]
    public async Task FindExact_DoesNotInvokeEmbedder()
    {
        using var db = new TranslationDb();
        db.Seed(Entry("<0>Hello</0>", "<0>Salam</0>"));
        var embedder = new FakeEmbedder();
        var repo = new TranslationMemory(db.Context, embedder);

        await repo.FindExactAsync(Lookup("<0>Hello</0>"), CancellationToken.None);

        Assert.Equal(0, embedder.CallCount);
    }

    [Fact]
    public async Task GetExamples_EmptyMemory_ReturnsNoExamples()
    {
        using var db = new TranslationDb();
        var repo = new TranslationMemory(db.Context, new FakeEmbedder());

        var examples = (
            await repo.GetExamplesAsync(Query("anything"), CancellationToken.None)
        ).ExpectSucc();

        Assert.Empty(examples);
    }

    [Fact]
    public async Task GetExamples_FuzzyHitAboveThreshold_IncludedAsFuzzyExample()
    {
        using var db = new TranslationDb();
        db.Seed(Entry("The quick brown fox jumped", "target-fuzzy"));
        var repo = new TranslationMemory(db.Context, new FakeEmbedder());

        var examples = (
            await repo.GetExamplesAsync(Query("The quick brown fox jumps"), CancellationToken.None)
        ).ExpectSucc();

        var example = Assert.Single(examples);
        Assert.Equal(ExampleTier.Fuzzy, example.Tier);
        Assert.Equal("target-fuzzy", example.Target);
    }

    [Fact]
    public async Task GetExamples_FuzzyFillsAllSlots_SkipsEmbedder()
    {
        using var db = new TranslationDb();
        db.Seed(Entry("The quick brown fox jumped", "t", embedding: [1f, 0f, 0f]));
        var embedder = new FakeEmbedder();
        var repo = new TranslationMemory(db.Context, embedder);

        var examples = (
            await repo.GetExamplesAsync(
                Query("The quick brown fox jumps", k: 1),
                CancellationToken.None
            )
        ).ExpectSucc();

        Assert.Single(examples);
        Assert.Equal(ExampleTier.Fuzzy, examples[0].Tier);
        Assert.Equal(0, embedder.CallCount);
    }

    [Fact]
    public async Task GetExamples_BelowFuzzyThreshold_SelectedAsSemanticNotFuzzy()
    {
        using var db = new TranslationDb();
        db.Seed(Entry("completely different text here", "t", embedding: [1f, 0f, 0f]));
        var embedder = new FakeEmbedder(
            new Dictionary<string, float[]> { ["short query"] = [1f, 0f, 0f] }
        );
        var repo = new TranslationMemory(db.Context, embedder);

        var examples = (
            await repo.GetExamplesAsync(Query("short query"), CancellationToken.None)
        ).ExpectSucc();

        var example = Assert.Single(examples);
        Assert.Equal(ExampleTier.Semantic, example.Tier);
        Assert.Equal(1, embedder.CallCount);
    }

    [Fact]
    public async Task GetExamples_SemanticTier_RanksByCosine()
    {
        using var db = new TranslationDb();
        db.Seed(
            Entry("source-p unrelated", "TP", embedding: [1f, 0f, 0f]),
            Entry("source-q different words", "TQ", embedding: [0f, 1f, 0f]),
            Entry("source-r other content", "TR", embedding: [0f, 0f, 1f])
        );
        var embedder = new FakeEmbedder(
            new Dictionary<string, float[]> { ["zzz query"] = [1f, 0.5f, 0f] }
        );
        var repo = new TranslationMemory(db.Context, embedder);

        var examples = (
            await repo.GetExamplesAsync(Query("zzz query", k: 2), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal(2, examples.Count);
        Assert.All(examples, e => Assert.Equal(ExampleTier.Semantic, e.Tier));
        Assert.Equal("TP", examples[0].Target);
        Assert.Equal("TQ", examples[1].Target);
        Assert.Equal(1, embedder.CallCount);
    }

    [Fact]
    public async Task GetExamples_SkipsCandidatesWithNullEmbedding()
    {
        using var db = new TranslationDb();
        db.Seed(
            Entry("alpha source text", "TA", embedding: [1f, 0f, 0f]),
            Entry("beta source text", "TB", embedding: null)
        );
        var embedder = new FakeEmbedder(
            new Dictionary<string, float[]> { ["query text here"] = [1f, 0f, 0f] }
        );
        var repo = new TranslationMemory(db.Context, embedder);

        var examples = (
            await repo.GetExamplesAsync(Query("query text here"), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal("TA", Assert.Single(examples).Target);
    }

    [Fact]
    public async Task GetExamples_FiltersByContentType()
    {
        using var db = new TranslationDb();
        db.Seed(
            Entry(
                "shared body one",
                "ARTICLE",
                embedding: [1f, 0f, 0f],
                contentType: ContentType.Article
            ),
            Entry(
                "shared body two",
                "SCIENTIST",
                embedding: [1f, 0f, 0f],
                contentType: ContentType.Scientist
            )
        );
        var embedder = new FakeEmbedder(
            new Dictionary<string, float[]> { ["some query"] = [1f, 0f, 0f] }
        );
        var repo = new TranslationMemory(db.Context, embedder);

        var examples = (
            await repo.GetExamplesAsync(Query("some query"), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal("SCIENTIST", Assert.Single(examples).Target);
    }

    [Fact]
    public async Task Add_ThenFindExact_RoundTrips()
    {
        using var db = new TranslationDb();
        var repo = new TranslationMemory(db.Context, new FakeEmbedder());

        var id = (
            await repo.AddAsync(Entry("<0>New</0>", "<0>Yeni</0>"), CancellationToken.None)
        ).ExpectSucc();
        Assert.False(string.IsNullOrEmpty(id));
        db.Context.ChangeTracker.Clear();

        var target = await repo.FindExactAsync(Lookup("<0>New</0>"), CancellationToken.None);
        Assert.Equal("<0>Yeni</0>", target);
    }
}
