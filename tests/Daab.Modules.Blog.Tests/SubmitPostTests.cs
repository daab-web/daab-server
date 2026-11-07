using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Features.Posts.CreatePost;
using Daab.Modules.Blog.Features.Posts.SubmitPost;
using Daab.Modules.Blog.Features.Posts.UpdatePost;
using Daab.Modules.Blog.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static Daab.Modules.Blog.Tests.TestSupport;

namespace Daab.Modules.Blog.Tests;

public sealed class SubmitPostTests
{
    [Fact]
    public async Task ApprovedContent_IsPublished_AfterVerifyingEveryLanguage()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context);
        var verifier = FakeVerifier.Approving();

        var result = await Handler(context, verifier)
            .Handle(new SubmitPostCommand(Scientist, post.Id), CancellationToken.None);

        var response = result.Match(r => r, e => throw new Xunit.Sdk.XunitException(e.Message));
        Assert.Equal(PostStatus.Published, response.Status);
        Assert.NotNull(context.Posts.Single().PublishedAt);
        Assert.Equal(["en", "az"], verifier.Requests.Select(r => r.Locale));
        Assert.Contains("Kvant", verifier.Requests[1].Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ARejectedTranslation_RejectsThePost_WithTheLanguageInTheFeedback()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context);
        var verifier = new FakeVerifier(r =>
            r.Locale == "az"
                ? ContentVerificationResult.Reject("Contains advertising.")
                : ContentVerificationResult.Approve()
        );

        var result = await Handler(context, verifier)
            .Handle(new SubmitPostCommand(Scientist, post.Id), CancellationToken.None);

        Assert.True(result.IsSucc);
        var saved = context.Posts.Single();
        Assert.Equal(PostStatus.Rejected, saved.Status);
        Assert.Null(saved.PublishedAt);
        Assert.Equal("[AZ] Contains advertising.", saved.VerificationFeedback);
    }

    [Fact]
    public async Task TooShortContent_IsRejected_WithoutCallingTheVerifierForThatLanguage()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, azBody: "Qısa.");
        var verifier = FakeVerifier.Approving();

        await Handler(context, verifier)
            .Handle(new SubmitPostCommand(Scientist, post.Id), CancellationToken.None);

        Assert.Equal(["en"], verifier.Requests.Select(r => r.Locale));
        var saved = context.Posts.Single();
        Assert.Equal(PostStatus.Rejected, saved.Status);
        Assert.StartsWith("[AZ]", saved.VerificationFeedback, StringComparison.Ordinal);
    }

    [Fact]
    public async Task VerifierUnavailable_Fails_AndLeavesPostAsDraft()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context);

        var result = await Handler(context, FakeVerifier.Failing())
            .Handle(new SubmitPostCommand(Scientist, post.Id), CancellationToken.None);

        Assert.True(result.IsFail);
        Assert.Equal(503, result.Match(_ => 0, e => e.Code));
        Assert.Equal(PostStatus.Draft, context.Posts.Single().Status);
    }

    [Fact]
    public async Task OnlyTheAuthor_CanSubmit()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context);

        var result = await Handler(context, FakeVerifier.Approving())
            .Handle(new SubmitPostCommand(Reader, post.Id), CancellationToken.None);

        Assert.Equal(403, result.Match(_ => 0, e => e.Code));
    }

    [Fact]
    public async Task EditingAPublishedPost_UnpublishesItUntilVerifiedAgain()
    {
        await using var context = NewContext();
        var post = await AddPostAsync(context, published: true);

        var request = new UpdatePostRequest(post.Id, "az", BothLanguages());
        var result = await new UpdatePostCommandHandler(context, Locales).Handle(
            new UpdatePostCommand(Scientist, request),
            CancellationToken.None
        );

        Assert.True(result.IsSucc);
        var saved = context.Posts.Single();
        Assert.Equal(PostStatus.Draft, saved.Status);
        Assert.Equal("az", saved.SourceLocale);
        Assert.Null(saved.PublishedAt);
    }

    [Fact]
    public async Task CreatingAPost_RequiresEveryLanguage()
    {
        await using var context = NewContext();
        var englishOnly = new CreatePostRequest("en", [BothLanguages()[0]]);

        var result = await new CreatePostCommandHandler(context, Locales).Handle(
            new CreatePostCommand(Scientist, englishOnly),
            CancellationToken.None
        );

        Assert.Equal(400, result.Match(_ => 0, e => e.Code));
        Assert.Contains("az", result.Match(_ => "", e => e.Message), StringComparison.Ordinal);
        Assert.Empty(context.Posts);
    }

    [Fact]
    public async Task CreatingAPost_StoresBothLanguages_AndSlugsTheOriginalTitle()
    {
        await using var context = NewContext();

        var result = await new CreatePostCommandHandler(context, Locales).Handle(
            new CreatePostCommand(Scientist, new CreatePostRequest("en", BothLanguages())),
            CancellationToken.None
        );

        var created = result.Match(r => r, e => throw new Xunit.Sdk.XunitException(e.Message));
        Assert.StartsWith("on-quantum-dots-", created.Slug, StringComparison.Ordinal);
        Assert.Equal(2, context.PostTranslations.Count());
        Assert.Equal("Kvant nöqtələri haqqında", context.Posts.Single().In("az")!.Title);
    }

    private static SubmitPostCommandHandler Handler(
        Persistence.BlogDbContext context,
        FakeVerifier verifier
    ) => new(context, verifier, Locales, NullLogger<SubmitPostCommandHandler>.Instance);
}
