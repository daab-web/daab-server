using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Blog.Features.Posts.GetPublishedPosts;

public sealed class GetPublishedPostsQueryHandler(BlogDbContext context)
    : IRequestHandler<GetPublishedPostsQuery, Fin<PagedResponse<PostSummaryResponse>>>
{
    public async Task<Fin<PagedResponse<PostSummaryResponse>>> Handle(
        GetPublishedPostsQuery request,
        CancellationToken cancellationToken
    )
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var query = context.Posts.AsNoTracking().Where(p => p.Status == PostStatus.Published);

        if (!string.IsNullOrWhiteSpace(request.ScientistId))
        {
            query = query.Where(p => p.ScientistId == request.ScientistId);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostSummaryResponse(
                p.Id,
                p.Slug,
                p.Title,
                p.Excerpt,
                p.Locale,
                p.AuthorId,
                p.AuthorName,
                p.ScientistId,
                p.PublishedAt,
                p.Comments.Count
            ))
            .ToListAsync(cancellationToken);

        return PagedResponse<PostSummaryResponse>.Create(items, page, pageSize, total);
    }
}
