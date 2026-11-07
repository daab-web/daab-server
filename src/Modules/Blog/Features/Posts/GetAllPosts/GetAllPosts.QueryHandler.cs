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

        var posts = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Translations)
            .Select(p => new { Post = p, CommentCount = p.Comments.Count })
            .ToListAsync(cancellationToken);

        var items = posts.ConvertAll(row => new AdminPostResponse(
            row.Post.Id,
            row.Post.Slug,
            row.Post.In(row.Post.SourceLocale)?.Title ?? string.Empty,
            row.Post.SourceLocale,
            [.. row.Post.Translations.Select(t => t.Locale).Order(StringComparer.Ordinal)],
            row.Post.AuthorId,
            row.Post.AuthorName,
            row.Post.Status,
            row.Post.VerificationFeedback,
            row.Post.UpdatedAt,
            row.Post.PublishedAt,
            row.CommentCount
        ));

        return PagedResponse<AdminPostResponse>.Create(items, page, pageSize, total);
    }
}
