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
    ILogger<SubmitPostCommandHandler> logger
) : IRequestHandler<SubmitPostCommand, Fin<SubmitPostResponse>>
{
    public async Task<Fin<SubmitPostResponse>> Handle(
        SubmitPostCommand request,
        CancellationToken cancellationToken
    )
    {
        var post = await context.Posts.SingleOrDefaultAsync(
            p => p.Id == request.PostId,
            cancellationToken
        );

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

        var body = LexicalText.Extract(post.EditorState);

        var verdict = ContentRules.Check(post.Title, body);
        if (verdict.Approved)
        {
            var verification = await verifier.VerifyAsync(
                new ContentVerificationRequest(post.Title, post.Excerpt, body, post.Locale),
                cancellationToken
            );

            if (verification.IsFail)
            {
                // Verification could not run: leave the post untouched so the author can retry.
                logger.LogWarning(
                    "Content verification failed for post {PostId}: {Error}",
                    post.Id,
                    verification.Match(_ => string.Empty, e => e.Message)
                );
                return Error.New(
                    StatusCodes.Status503ServiceUnavailable,
                    "Content verification is unavailable right now. Please try again later"
                );
            }

            verdict = verification.Match(v => v, _ => verdict);
        }

        post.ApplyVerification(
            verdict.Approved,
            verdict.Approved ? null : string.Join("\n", verdict.Reasons)
        );
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Post {PostId} verification result: {Status}", post.Id, post.Status);

        return post.ToSubmitResponse();
    }
}
