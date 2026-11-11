using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Features.Posts;
using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using Daab.SharedKernel.Options;
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

    public static readonly BlogLocales Locales = new(
        Microsoft.Extensions.Options.Options.Create(
            new LocaleOptions { SupportedLocales = ["en", "az"] }
        )
    );

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

    public static readonly string LongBodyEn = string.Concat(
        Enumerable.Repeat("Quantum dots emit light at tunable wavelengths. ", 5)
    );

    public static readonly string LongBodyAz = string.Concat(
        Enumerable.Repeat("Kvant nöqtələri tənzimlənən dalğa uzunluqlarında işıq yayır. ", 5)
    );

    public static List<PostTranslationInput> BothLanguages(
        string? enBody = null,
        string? azBody = null
    ) =>
        [
            new("en", "On quantum dots", "A short excerpt", EditorState(enBody ?? LongBodyEn)),
            new("az", "Kvant nöqtələri haqqında", "Qısa xülasə", EditorState(azBody ?? LongBodyAz)),
        ];

    public static async Task<Post> AddPostAsync(
        BlogDbContext context,
        bool published = false,
        string? enBody = null,
        string? azBody = null
    )
    {
        var post = Post.Create(
            Scientist.Id,
            Scientist.Name,
            Scientist.ScientistId,
            "en",
            [
                .. BothLanguages(enBody, azBody)
                    .Select(t => new PostContent(t.Locale, t.Title, t.Excerpt, t.EditorState)),
            ]
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

internal sealed class FakeVerifier(
    Func<ContentVerificationRequest, Fin<ContentVerificationResult>> verify
) : IContentVerifier
{
    public List<ContentVerificationRequest> Requests { get; } = [];

    public static FakeVerifier Approving() => new(_ => ContentVerificationResult.Approve());

    public static FakeVerifier Rejecting(string reason) =>
        new(_ => ContentVerificationResult.Reject(reason));

    public static FakeVerifier Failing() =>
        new(_ => FinFail<ContentVerificationResult>(Error.New(502, "model down")));

    public Task<Fin<ContentVerificationResult>> VerifyAsync(
        ContentVerificationRequest request,
        CancellationToken cancellationToken
    )
    {
        Requests.Add(request);
        return Task.FromResult(verify(request));
    }
}

internal sealed class FakeTranslator(bool fail = false) : ITextTranslator
{
    public int Calls { get; private set; }
    public IReadOnlyList<string>? LastTexts { get; private set; }

    public Task<Fin<IReadOnlyList<string>>> TranslateAsync(
        IReadOnlyList<string> texts,
        string sourceLocale,
        string targetLocale,
        CancellationToken cancellationToken
    )
    {
        Calls++;
        LastTexts = texts;
        return Task.FromResult(
            fail
                ? FinFail<IReadOnlyList<string>>(Error.New(502, "model down"))
                : FinSucc<IReadOnlyList<string>>([.. texts.Select(t => $"[{targetLocale}] {t}")])
        );
    }
}
