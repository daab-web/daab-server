using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace Daab.Modules.Blog.Tests;

internal static class TestSupport
{
    public static readonly BlogUser Scientist = new("01SCIENTIST", "Dr. Test", "SCI-1", false);
    public static readonly BlogUser Reader = new("01READER", "Reader", null, false);
    public static readonly BlogUser Admin = new("01ADMIN", "Admin", null, true);

    public static BlogDbContext NewContext() =>
        new(
            new DbContextOptionsBuilder<BlogDbContext>()
                .UseInMemoryDatabase($"blog_{Guid.NewGuid():N}")
                .Options
        );

    public static string EditorState(string text) =>
        $$$"""
            {"root":{"children":[{"children":[{"text":"{{{text}}}","type":"text"}],"type":"paragraph"}],"type":"root"}}
            """;

    public static readonly string LongBody = string.Concat(
        Enumerable.Repeat("Quantum dots emit light at tunable wavelengths. ", 5)
    );

    public static async Task<Post> AddPostAsync(
        BlogDbContext context,
        bool published = false,
        string? body = null
    )
    {
        var post = Post.Create(
            Scientist.Id,
            Scientist.Name,
            Scientist.ScientistId,
            "On quantum dots",
            "A short excerpt",
            EditorState(body ?? LongBody),
            "en"
        );

        if (published)
        {
            post.ApplyVerification(true, null);
        }

        context.Posts.Add(post);
        await context.SaveChangesAsync();
        return post;
    }
}

internal sealed class FakeVerifier(Fin<ContentVerificationResult> result) : IContentVerifier
{
    public int Calls { get; private set; }
    public ContentVerificationRequest? LastRequest { get; private set; }

    public static FakeVerifier Approving() => new(ContentVerificationResult.Approve());

    public static FakeVerifier Rejecting(string reason) =>
        new(ContentVerificationResult.Reject(reason));

    public static FakeVerifier Failing() =>
        new(FinFail<ContentVerificationResult>(Error.New(502, "model down")));

    public Task<Fin<ContentVerificationResult>> VerifyAsync(
        ContentVerificationRequest request,
        CancellationToken cancellationToken
    )
    {
        Calls++;
        LastRequest = request;
        return Task.FromResult(result);
    }
}

internal sealed class FakeLanguageService(string detected = "az", bool fail = false)
    : ICommentLanguageService
{
    public int TranslateCalls { get; private set; }

    public Task<Fin<string>> DetectLanguageAsync(
        string text,
        CancellationToken cancellationToken
    ) => Task.FromResult(fail ? FinFail<string>(Error.New(502, "model down")) : FinSucc(detected));

    public Task<Fin<string>> TranslateAsync(
        string text,
        string targetLocale,
        CancellationToken cancellationToken
    )
    {
        TranslateCalls++;
        return Task.FromResult(
            fail
                ? FinFail<string>(Error.New(502, "model down"))
                : FinSucc($"[{targetLocale}] {text}")
        );
    }
}
