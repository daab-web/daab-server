using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Blog.Features.Posts.GetPost;

public sealed class GetPostQueryHandler(BlogDbContext context)
    : IRequestHandler<GetPostQuery, Fin<PostResponse>>
{
    public async Task<Fin<PostResponse>> Handle(
        GetPostQuery request,
        CancellationToken cancellationToken
    )
    {
        var post = await context
            .Posts.AsNoTracking()
            .SingleOrDefaultAsync(
                p => p.Id == request.IdOrSlug || p.Slug == request.IdOrSlug,
                cancellationToken
            );

        var visible =
            post is not null
            && (
                post.Status == PostStatus.Published
                || (request.Viewer is { } viewer && (post.IsOwnedBy(viewer.Id) || viewer.IsAdmin))
            );

        return visible
            ? post!.ToResponse()
            : Error.New(StatusCodes.Status404NotFound, "Post not found");
    }
}
