using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Unit = LanguageExt.Unit;

namespace Daab.Modules.Blog.Features.Comments.DeleteComment;

public sealed class DeleteCommentCommandHandler(BlogDbContext context)
    : IRequestHandler<DeleteCommentCommand, Fin<Unit>>
{
    public async Task<Fin<Unit>> Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        var comment = await context
            .Comments.Include(c => c.Post)
            .SingleOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Comment not found");
        }

        // The comment author, the post author (moderating their own post) or an admin.
        var allowed =
            comment.IsOwnedBy(request.User.Id)
            || comment.Post.IsOwnedBy(request.User.Id)
            || request.User.IsAdmin;

        if (!allowed)
        {
            return Error.New(StatusCodes.Status403Forbidden, "You cannot delete this comment");
        }

        context.Comments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Default;
    }
}
