using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Daab.Modules.Blog.Features.Comments.AddComment;

public sealed class AddCommentCommandHandler(
    BlogDbContext context,
    ICommentLanguageService languageService,
    ILogger<AddCommentCommandHandler> logger
) : IRequestHandler<AddCommentCommand, Fin<CommentResponse>>
{
    public async Task<Fin<CommentResponse>> Handle(
        AddCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        var published = await context.Posts.AnyAsync(
            p => p.Id == request.PostId && p.Status == PostStatus.Published,
            cancellationToken
        );

        if (!published)
        {
            return Error.New(StatusCodes.Status404NotFound, "Post not found");
        }

        // Detection is best effort: a comment is never rejected because it failed.
        var detected = await languageService.DetectLanguageAsync(
            request.Content,
            cancellationToken
        );
        var language = detected.Match(
            code => code,
            err =>
            {
                logger.LogWarning("Comment language detection failed: {Error}", err.Message);
                return (string?)null;
            }
        );

        var comment = Comment.Create(
            request.PostId,
            request.Author.Id,
            request.Author.Name,
            request.Content,
            language
        );

        await context.Comments.AddAsync(comment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return comment.ToResponse();
    }
}
