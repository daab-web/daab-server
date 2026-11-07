using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Blog.Features.Posts.UpdatePost;

public sealed class UpdatePostCommandHandler(BlogDbContext context)
    : IRequestHandler<UpdatePostCommand, Fin<PostResponse>>
{
    public async Task<Fin<PostResponse>> Handle(
        UpdatePostCommand request,
        CancellationToken cancellationToken
    )
    {
        var (author, content) = request;

        var post = await context.Posts.SingleOrDefaultAsync(
            p => p.Id == content.Id,
            cancellationToken
        );

        if (post is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Post not found");
        }

        if (!post.IsOwnedBy(author.Id))
        {
            return Error.New(StatusCodes.Status403Forbidden, "Only the author can edit this post");
        }

        // Editing moves the post back to draft; it must pass verification again to be published.
        post.Update(content.Title, content.Excerpt, content.EditorState, content.Locale);
        await context.SaveChangesAsync(cancellationToken);

        return post.ToResponse();
    }
}
