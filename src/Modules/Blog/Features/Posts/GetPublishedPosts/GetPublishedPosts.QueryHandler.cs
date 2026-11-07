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

        var posts = await query
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Translations)
            .Select(p => new { Post = p, CommentCount = p.Comments.Count })
            .ToListAsync(cancellationToken);

        var items = posts.ConvertAll(row =>
        {
            var content = row.Post.In(request.Locale)!;
            return new PostSummaryResponse(
                row.Post.Id,
                row.Post.Slug,
                content.Locale,
                content.Title,
                content.Excerpt,
                row.Post.AuthorId,
                row.Post.AuthorName,
                row.Post.ScientistId,
                row.Post.PublishedAt,
                row.CommentCount
            );
        });

        return PagedResponse<PostSummaryResponse>.Create(items, page, pageSize, total);
    }
}
