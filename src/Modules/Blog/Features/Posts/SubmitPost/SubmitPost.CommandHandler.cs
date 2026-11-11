using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Daab.Modules.Blog.Features.Posts.SubmitPost;

public sealed class SubmitPostCommandHandler(
    BlogDbContext context,
    IContentVerifier verifier,
    BlogLocales locales,
    ILogger<SubmitPostCommandHandler> logger
) : IRequestHandler<SubmitPostCommand, Fin<SubmitPostResponse>>
{
    public async Task<Fin<SubmitPostResponse>> Handle(
        SubmitPostCommand request,
        CancellationToken cancellationToken
    )
    {
        var post = await context
            .Posts.Include(p => p.Translations)
            .SingleOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Post not found");
        }

        if (!post.IsOwnedBy(request.Author.Id))
        {
            return Error.New(
                StatusCodes.Status403Forbidden,
                "Only the author can publish this post"
            );
        }

        if (post.Status == PostStatus.Published)
        {
            return post.ToSubmitResponse();
        }

        var complete = locales.RequireAll(
            post.SourceLocale,
            post.Translations.Select(t => t.Locale)
        );
        if (complete.IsFail)
        {
            return complete.Match(_ => Error.New("unreachable"), e => e);
        }

        // Every language is published, so every language must pass verification.
        var reasons = new List<string>();
        foreach (
            var translation in post.Translations.OrderBy(t => t.Locale == post.SourceLocale ? 0 : 1)
        )
        {
            var body = LexicalText.Extract(translation.EditorState);
            var verdict = ContentRules.Check(translation.Title, body);

            if (verdict.Approved)
            {
                var verification = await verifier.VerifyAsync(
                    new ContentVerificationRequest(
                        translation.Title,
                        translation.Excerpt,
                        body,
                        translation.Locale
                    ),
                    cancellationToken
                );

                if (verification.IsFail)
                {
                    // Verification could not run: leave the post untouched so the author can retry.
                    logger.LogWarning(
                        "Content verification failed for post {PostId} ({Locale}): {Error}",
                        post.Id,
                        translation.Locale,
                        verification.Match(_ => string.Empty, e => e.Message)
                    );
                    return Error.New(
                        StatusCodes.Status503ServiceUnavailable,
                        "Content verification is unavailable right now. Please try again later"
                    );
                }

                verdict = verification.Match(v => v, _ => verdict);
            }

            reasons.AddRange(
                verdict.Reasons.Select(r => $"[{translation.Locale.ToUpperInvariant()}] {r}")
            );
        }

        var approved = reasons.Count == 0;
        post.ApplyVerification(approved, approved ? null : string.Join("\n", reasons));
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Post {PostId} verification result: {Status}", post.Id, post.Status);

        return post.ToSubmitResponse();
    }
}
