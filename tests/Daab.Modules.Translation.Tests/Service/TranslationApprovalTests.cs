using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Models;
using Daab.Modules.Translation.Service;
using Daab.Modules.Translation.Tests.Memory;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Translation.Tests.Service;

public sealed class TranslationApprovalTests
{
    static ApprovalRequest Approval(string sourceJson, string targetJson) =>
        new(sourceJson, targetJson, ContentType.Scientist, "en", "az");

    // Approval never calls the model, so any translator will do; the embedder is what matters.
    static TranslationService Service(TranslationDb db, FakeEmbedder embedder) =>
        new(
            FakeTranslator.Returning(),
            new TranslationMemory(db.Context, embedder),
            embedder,
            FakeGlossaryProvider.Empty
        );

    static string TextOf(string json, int blockIndex) =>
        LexicalDocs.Children(json, blockIndex)[0]!["text"]!.GetValue<string>();

    [Fact]
    public async Task Approve_NewDocument_EmbedsAndStoresEveryBlock()
    {
        using var db = new TranslationDb();
        var embedder = new FakeEmbedder();
        var source = LexicalDocs.Doc(
            LexicalDocs.Paragraph(LexicalDocs.Text("Hello")),
            LexicalDocs.Paragraph(LexicalDocs.Text("World"))
        );
        var target = LexicalDocs.Doc(
            LexicalDocs.Paragraph(LexicalDocs.Text("Salam")),
            LexicalDocs.Paragraph(LexicalDocs.Text("Dünya"))
        );

        var result = (
            await Service(db, embedder)
                .ApproveDocumentAsync(Approval(source, target), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal(2, result.BlockCount);
        Assert.Equal(2, result.Added);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(2, embedder.CallCount);

        var stored = await db
            .Context.Entries.AsNoTracking()
            .OrderBy(e => e.TargetText)
            .ToListAsync();
        Assert.Equal(2, stored.Count);
        Assert.All(stored, e => Assert.NotNull(e.Embedding));
    }

    [Fact]
    public async Task Approve_BlockAlreadyRemembered_SkipsAndDoesNotEmbed()
    {
        using var db = new TranslationDb();
        db.Seed(
            TranslationMemoryEntry.Create(
                "Hello",
                "Salam",
                ContentType.Scientist,
                BlockType.Paragraph,
                "en",
                "az"
            )
        );
        var embedder = new FakeEmbedder();
        var source = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello")));
        var target = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Salam")));

        var result = (
            await Service(db, embedder)
                .ApproveDocumentAsync(Approval(source, target), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal(0, result.Added);
        Assert.Equal(1, result.Skipped);
        Assert.Equal(0, embedder.CallCount);
    }

    [Fact]
    public async Task Approve_BlockCountMismatch_FailsAndStoresNothing()
    {
        using var db = new TranslationDb();
        var embedder = new FakeEmbedder();
        var source = LexicalDocs.Doc(
            LexicalDocs.Paragraph(LexicalDocs.Text("Hello")),
            LexicalDocs.Paragraph(LexicalDocs.Text("World"))
        );
        var target = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Salam")));

        var error = (
            await Service(db, embedder)
                .ApproveDocumentAsync(Approval(source, target), CancellationToken.None)
        ).ExpectFail();

        Assert.Equal(0, embedder.CallCount);
        Assert.Empty(db.Context.Entries.AsNoTracking());
        Assert.Contains("blocks", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Approve_TargetDropsTag_FailsAndStoresNothing()
    {
        using var db = new TranslationDb();
        var embedder = new FakeEmbedder();
        // The source has a bold run (<0>…</0>); the approved target drops it.
        var source = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello", format: 1)));
        var target = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Salam")));

        var error = (
            await Service(db, embedder)
                .ApproveDocumentAsync(Approval(source, target), CancellationToken.None)
        ).ExpectFail();

        Assert.Empty(db.Context.Entries.AsNoTracking());
        Assert.Contains("Tag", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Approve_InvalidSourceJson_FailsBeforeEmbedding()
    {
        using var db = new TranslationDb();
        var embedder = new FakeEmbedder();
        var target = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Salam")));

        var error = (
            await Service(db, embedder)
                .ApproveDocumentAsync(Approval("{ not valid json", target), CancellationToken.None)
        ).ExpectFail();

        Assert.Equal(0, embedder.CallCount);
        Assert.Empty(db.Context.Entries.AsNoTracking());
        Assert.False(string.IsNullOrEmpty(error.Message));
    }

    [Fact]
    public async Task Approve_ThenTranslate_ServesApprovedTargetFromCache()
    {
        // The flywheel: an approved pair is served verbatim from the memory on the next translate,
        // with no model call. This is the whole point of the write-back loop.
        using var db = new TranslationDb();
        var embedder = new FakeEmbedder();
        var service = Service(db, embedder);
        var source = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello")));
        var target = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Salam")));

        _ = (
            await service.ApproveDocumentAsync(Approval(source, target), CancellationToken.None)
        ).ExpectSucc();

        var draft = (
            await service.TranslateDocumentAsync(
                new TranslationRequest(source, ContentType.Scientist, "en", "az"),
                CancellationToken.None
            )
        ).ExpectSucc();

        Assert.Equal(1, draft.ExactHits);
        Assert.Equal(0, draft.ModelTranslated);
        Assert.Equal("Salam", TextOf(draft.TranslatedJson, 0));
    }
}
