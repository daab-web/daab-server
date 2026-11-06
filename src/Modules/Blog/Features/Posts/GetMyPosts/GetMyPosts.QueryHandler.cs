using Daab.Modules.Blog.Persistence;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Blog.Features.Posts.GetMyPosts;

public sealed class GetMyPostsQueryHandler(BlogDbContext context)
    : IRequestHandler<GetMyPostsQuery, Fin<List<MyPostResponse>>>
{
    public async Task<Fin<List<MyPostResponse>>> Handle(
        GetMyPostsQuery request,
        CancellationToken cancellationToken
    )
    {
        return await context
            .Posts.AsNoTracking()
            .Where(p => p.AuthorId == request.AuthorId)
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => new MyPostResponse(
                p.Id,
                p.Slug,
                p.Title,
                p.Excerpt,
                p.Locale,
                p.Status,
                p.VerificationFeedback,
                p.CreatedAt,
                p.UpdatedAt,
                p.PublishedAt,
                p.Comments.Count
            ))
            .ToListAsync(cancellationToken);
    }
}
