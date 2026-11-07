using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Unit = LanguageExt.Unit;

namespace Daab.Modules.Blog.Features.Posts.DeletePost;

public sealed class DeletePostCommandHandler(BlogDbContext context)
    : IRequestHandler<DeletePostCommand, Fin<Unit>>
{
    public async Task<Fin<Unit>> Handle(
        DeletePostCommand request,
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

        if (!post.IsOwnedBy(request.User.Id) && !request.User.IsAdmin)
        {
            return Error.New(StatusCodes.Status403Forbidden, "You cannot delete this post");
        }

        context.Posts.Remove(post);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Default;
    }
}
