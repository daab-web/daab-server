using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Models;
using Daab.Modules.Translation.Service;
using Daab.Modules.Translation.Tests.Memory;

namespace Daab.Modules.Translation.Tests.Service;

public sealed class TranslationServiceTests
{
    static TranslationRequest Request(string editorStateJson) =>
        new(editorStateJson, ContentType.Scientist, "en", "az");

    static TranslationMemoryEntry Cache(string source, string target) =>
        TranslationMemoryEntry.Create(
            source,
            target,
            ContentType.Scientist,
            BlockType.Paragraph,
            "en",
            "az",
            null
        );

    static TranslationService Service(TranslationDb db, FakeTranslator translator) =>
        new(
            translator,
            new TranslationMemory(db.Context, new FakeEmbedder()),
            FakeGlossaryProvider.Empty
        );

    static string TextOf(string json, int blockIndex) =>
        LexicalDocs.Children(json, blockIndex)[0]!["text"]!.GetValue<string>();

    [Fact]
    public async Task Translate_WholeDocumentCached_SkipsModelAndServesCache()
    {
        using var db = new TranslationDb();
        db.Seed(Cache("Hello", "Salam"), Cache("World", "Dünya"));
        var translator = FakeTranslator.Returning();
        var doc = LexicalDocs.Doc(
            LexicalDocs.Paragraph(LexicalDocs.Text("Hello")),
            LexicalDocs.Paragraph(LexicalDocs.Text("World"))
        );

        var draft = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal(0, translator.CallCount);
        Assert.Equal(2, draft.ExactHits);
        Assert.Equal(0, draft.ModelTranslated);
        Assert.Equal("Salam", TextOf(draft.TranslatedJson, 0));
        Assert.Equal("Dünya", TextOf(draft.TranslatedJson, 1));
    }

    [Fact]
    public async Task Translate_MissBlock_CallsModelAndReinserts()
    {
        using var db = new TranslationDb();
        var translator = FakeTranslator.Returning("Salam");
        var doc = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello")));

        var draft = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal(1, translator.CallCount);
        Assert.Equal(1, draft.ModelTranslated);
        Assert.Equal("Salam", TextOf(draft.TranslatedJson, 0));
    }

    [Fact]
    public async Task Translate_PartialCache_TranslatesOnlyMissesAndMerges()
    {
        using var db = new TranslationDb();
        db.Seed(Cache("Alpha", "AlphaAz"));
        var translator = FakeTranslator.Returning("BetaAz");
        var doc = LexicalDocs.Doc(
            LexicalDocs.Paragraph(LexicalDocs.Text("Alpha")),
            LexicalDocs.Paragraph(LexicalDocs.Text("Beta"))
        );

        var draft = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal(1, draft.ExactHits);
        Assert.Equal(1, draft.ModelTranslated);
        Assert.Equal("AlphaAz", TextOf(draft.TranslatedJson, 0));
        Assert.Equal("BetaAz", TextOf(draft.TranslatedJson, 1));
        // Only the miss is sent to the model.
        Assert.Contains("Beta", translator.LastUser!, StringComparison.Ordinal);
        Assert.DoesNotContain("Alpha", translator.LastUser!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Translate_ModelDropsTag_FailsValidation()
    {
        using var db = new TranslationDb();
        // A bold run becomes <0>...</0>; the model returns untagged text.
        var translator = FakeTranslator.Returning("Salam");
        var doc = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello", format: 1)));

        var error = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectFail();

        Assert.Contains("Tag validation", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Translate_ModelReturnsWrongCount_Fails()
    {
        using var db = new TranslationDb();
        var translator = FakeTranslator.Returning("only-one");
        var doc = LexicalDocs.Doc(
            LexicalDocs.Paragraph(LexicalDocs.Text("Alpha")),
            LexicalDocs.Paragraph(LexicalDocs.Text("Beta"))
        );

        var error = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectFail();

        Assert.Contains("translations for", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Translate_ModelReturnsNonArray_Fails()
    {
        using var db = new TranslationDb();
        var translator = FakeTranslator.Raw("{\"unexpected\":true}");
        var doc = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello")));

        var error = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectFail();

        Assert.Contains("not a JSON array", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Translate_InvalidEditorState_Fails()
    {
        using var db = new TranslationDb();
        var translator = FakeTranslator.Returning();

        var error = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request("{ not valid json"), CancellationToken.None)
        ).ExpectFail();

        // Extract failed before any model call.
        Assert.Equal(0, translator.CallCount);
        Assert.False(string.IsNullOrEmpty(error.Message));
    }

    [Fact]
    public async Task Translate_TagDroppedThenFixed_RetriesAndSucceeds()
    {
        using var db = new TranslationDb();
        // First attempt drops the <0> tag; the retry restores it.
        var translator = FakeTranslator.Sequence(["Salam"], ["<0>Salam</0>"]);
        var doc = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello", format: 1)));

        var draft = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal(2, translator.CallCount);
        Assert.Equal("Salam", TextOf(draft.TranslatedJson, 0));
        // The retry prompt fed the tag error back.
        Assert.Contains("PROBLEM", translator.LastUser!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Translate_TagAlwaysBroken_FailsAfterRetries()
    {
        using var db = new TranslationDb();
        // MaxTagRetries defaults to 2 → three attempts, all broken.
        var translator = FakeTranslator.Sequence(["Salam"], ["Salam"], ["Salam"]);
        var doc = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello", format: 1)));

        var error = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectFail();

        Assert.Equal(3, translator.CallCount);
        Assert.Contains("after retries", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Translate_EmptyBlockThenFixed_Retries()
    {
        using var db = new TranslationDb();
        var translator = FakeTranslator.Sequence([""], ["Salam"]);
        var doc = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello")));

        var draft = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectSucc();

        Assert.Equal(2, translator.CallCount);
        Assert.Equal("Salam", TextOf(draft.TranslatedJson, 0));
    }

    [Fact]
    public async Task Translate_Success_ReturnsReExtractableLexical()
    {
        using var db = new TranslationDb();
        var translator = FakeTranslator.Returning("Salam");
        var doc = LexicalDocs.Doc(LexicalDocs.Paragraph(LexicalDocs.Text("Hello")));

        var draft = (
            await Service(db, translator)
                .TranslateDocumentAsync(Request(doc), CancellationToken.None)
        ).ExpectSucc();

        // The returned document round-trips back through the Lexical schema.
        var reExtracted = LexicalWalker.Extract(draft.TranslatedJson).ExpectSucc();
        Assert.Equal("Salam", reExtracted.Blocks[0].Source);
    }
}
