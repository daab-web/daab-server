using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Features.Comments.AddComment;
using Daab.Modules.Blog.Features.Comments.DeleteComment;
using Daab.Modules.Blog.Features.Translations.SuggestTranslation;
using static Daab.Modules.Blog.Tests.TestSupport;

namespace Daab.Modules.Blog.Tests;

public sealed class CommentTests
{
    private static readonly Dictionary<string, string> Both = new()
    {
        ["az"] = "  Çox maraqlı məqalədir!  ",
        ["en"] = "Very interesting article!",
    };

    [Fact]
    public async Task AddComment_StoresEveryLanguage_AndTheOriginal()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: true);

        var result = await AddHandler(context)
            .Handle(new AddCommentCommand(Reader, post.Id, "az", Both), CancellationToken.None);

        var comment = result.Match(c => c, e => throw new Xunit.Sdk.XunitException(e.Message));
        Assert.Equal("az", comment.SourceLocale);
        Assert.Equal("Çox maraqlı məqalədir!", comment.Texts["az"]);
        Assert.Equal("Very interesting article!", comment.Texts["en"]);
        Assert.Equal(Reader.Name, comment.AuthorName);
    }

    [Fact]
    public async Task AddComment_InOneLanguageOnly_IsRejected()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: true);

        var result = await AddHandler(context)
            .Handle(
                new AddCommentCommand(
                    Reader,
                    post.Id,
                    "en",
                    new Dictionary<string, string> { ["en"] = "Hello" }
                ),
                CancellationToken.None
            );

        Assert.Equal(400, result.Match(_ => 0, e => e.Code));
        Assert.Empty(context.Comments);
    }

    [Fact]
    public async Task AddComment_OnUnpublishedPost_IsNotFound()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: false);

        var result = await AddHandler(context)
            .Handle(new AddCommentCommand(Reader, post.Id, "az", Both), CancellationToken.None);

        Assert.Equal(404, result.Match(_ => 0, e => e.Code));
    }

    [Theory]
    [InlineData("reader", true)]
    [InlineData("post-author", true)]
    [InlineData("admin", true)]
    [InlineData("stranger", false)]
    public async Task DeleteComment_IsLimitedToAuthorsAndAdmins(string who, bool allowed)
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: true);
        var comment = (
            await AddHandler(context)
                .Handle(new AddCommentCommand(Reader, post.Id, "az", Both), CancellationToken.None)
        ).Match(c => c, e => throw new Xunit.Sdk.XunitException(e.Message));

        BlogUser user = who switch
        {
            "reader" => Reader,
            "post-author" => Scientist,
            "admin" => Admin,
            _ => new BlogUser("01STRANGER", "Stranger", null, false),
        };

        var result = await new DeleteCommentCommandHandler(context).Handle(
            new DeleteCommentCommand(user, comment.Id),
            CancellationToken.None
        );

        Assert.Equal(allowed, result.IsSucc);
        Assert.Equal(allowed ? 0 : 1, context.Comments.Count());
        Assert.Equal(allowed ? 0 : 2, context.CommentTexts.Count());
    }

    [Fact]
    public async Task SuggestTranslation_KeepsOrder_AndSkipsEmptyTexts()
    {
        var translator = new FakeTranslator();

        var result = await new SuggestTranslationCommandHandler(translator).Handle(
            new SuggestTranslationCommand("en", "az", ["Title", "  ", "Summary"]),
            CancellationToken.None
        );

        var suggestion = result.Match(s => s, e => throw new Xunit.Sdk.XunitException(e.Message));
        Assert.Equal(["[az] Title", "", "[az] Summary"], suggestion.Translations);
        Assert.Equal(["Title", "Summary"], translator.LastTexts);
    }

    [Fact]
    public async Task SuggestTranslation_WhenTheModelIsDown_IsUnavailable()
    {
        var result = await new SuggestTranslationCommandHandler(
            new FakeTranslator(fail: true)
        ).Handle(new SuggestTranslationCommand("en", "az", ["Hello"]), CancellationToken.None);

        Assert.Equal(503, result.Match(_ => 0, e => e.Code));
    }

    private static AddCommentCommandHandler AddHandler(Persistence.BlogDbContext context) =>
        new(context, Locales);
}
