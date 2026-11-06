using Daab.Modules.Blog.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Blog.Features.Posts.GetAllPosts;

public sealed class GetAllPostsQueryHandler(BlogDbContext context)
    : IRequestHandler<GetAllPostsQuery, Fin<PagedResponse<AdminPostResponse>>>
{
    public async Task<Fin<PagedResponse<AdminPostResponse>>> Handle(
        GetAllPostsQuery request,
        CancellationToken cancellationToken
    )
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = context.Posts.AsNoTracking();
        if (request.Status is { } status)
        {
            query = query.Where(p => p.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminPostResponse(
                p.Id,
                p.Slug,
                p.Title,
                p.Locale,
                p.AuthorId,
                p.AuthorName,
                p.Status,
                p.VerificationFeedback,
                p.UpdatedAt,
                p.PublishedAt,
                p.Comments.Count
            ))
            .ToListAsync(cancellationToken);

        return PagedResponse<AdminPostResponse>.Create(items, page, pageSize, total);
    }
}
