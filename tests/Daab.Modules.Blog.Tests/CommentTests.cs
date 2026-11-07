using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Features.Comments.AddComment;
using Daab.Modules.Blog.Features.Comments.DeleteComment;
using Daab.Modules.Blog.Features.Comments.TranslateComment;
using Microsoft.Extensions.Logging.Abstractions;
using static Daab.Modules.Blog.Tests.TestSupport;

namespace Daab.Modules.Blog.Tests;

public sealed class CommentTests
{
    [Fact]
    public async Task AddComment_StoresDetectedLanguage()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: true);

        var result = await AddHandler(context, new FakeLanguageService("az"))
            .Handle(
                new AddCommentCommand(Reader, post.Id, "  Çox maraqlı məqalədir!  "),
                CancellationToken.None
            );

        var comment = result.Match(c => c, e => throw new Xunit.Sdk.XunitException(e.Message));
        Assert.Equal("az", comment.Language);
        Assert.Equal("Çox maraqlı məqalədir!", comment.Content);
        Assert.Equal(Reader.Name, comment.AuthorName);
    }

    [Fact]
    public async Task AddComment_StillSaves_WhenDetectionFails()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: true);

        var result = await AddHandler(context, new FakeLanguageService(fail: true))
            .Handle(new AddCommentCommand(Reader, post.Id, "Nice"), CancellationToken.None);

        Assert.True(result.IsSucc);
        Assert.Null(context.Comments.Single().Language);
    }

    [Fact]
    public async Task AddComment_OnUnpublishedPost_IsNotFound()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: false);

        var result = await AddHandler(context, new FakeLanguageService())
            .Handle(new AddCommentCommand(Reader, post.Id, "Hello"), CancellationToken.None);

        Assert.Equal(404, result.Match(_ => 0, e => e.Code));
    }

    [Fact]
    public async Task Translate_IsCachedPerLocale()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: true);
        var language = new FakeLanguageService("az");
        var comment = (
            await AddHandler(context, language)
                .Handle(new AddCommentCommand(Reader, post.Id, "Salam"), CancellationToken.None)
        ).Match(c => c, e => throw new Xunit.Sdk.XunitException(e.Message));

        var handler = new TranslateCommentCommandHandler(context, language);
        var first = await handler.Handle(
            new TranslateCommentCommand(comment.Id, "en"),
            CancellationToken.None
        );
        var second = await handler.Handle(
            new TranslateCommentCommand(comment.Id, "en"),
            CancellationToken.None
        );

        Assert.Equal("[en] Salam", first.Match(t => t.Content, e => e.Message));
        Assert.Equal("[en] Salam", second.Match(t => t.Content, e => e.Message));
        Assert.Equal(1, language.TranslateCalls);
    }

    [Fact]
    public async Task Translate_IntoTheCommentsOwnLanguage_ReturnsOriginal()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: true);
        var language = new FakeLanguageService("az");
        var comment = (
            await AddHandler(context, language)
                .Handle(new AddCommentCommand(Reader, post.Id, "Salam"), CancellationToken.None)
        ).Match(c => c, e => throw new Xunit.Sdk.XunitException(e.Message));

        var result = await new TranslateCommentCommandHandler(context, language).Handle(
            new TranslateCommentCommand(comment.Id, "az"),
            CancellationToken.None
        );

        Assert.Equal("Salam", result.Match(t => t.Content, e => e.Message));
        Assert.Equal(0, language.TranslateCalls);
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
            await AddHandler(context, new FakeLanguageService())
                .Handle(new AddCommentCommand(Reader, post.Id, "Hi"), CancellationToken.None)
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
    }

    private static AddCommentCommandHandler AddHandler(
        Persistence.BlogDbContext context,
        ICommentLanguageService language
    ) => new(context, language, NullLogger<AddCommentCommandHandler>.Instance);
}
