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
        var posts = await context
            .Posts.AsNoTracking()
            .Where(p => p.AuthorId == request.AuthorId)
            .OrderByDescending(p => p.UpdatedAt)
            .Include(p => p.Translations)
            .Select(p => new { Post = p, CommentCount = p.Comments.Count })
            .ToListAsync(cancellationToken);

        return posts.ConvertAll(row => new MyPostResponse(
            row.Post.Id,
            row.Post.Slug,
            row.Post.In(row.Post.SourceLocale)?.Title ?? string.Empty,
            row.Post.SourceLocale,
            [.. row.Post.Translations.Select(t => t.Locale).Order(StringComparer.Ordinal)],
            row.Post.Status,
            row.Post.VerificationFeedback,
            row.Post.CreatedAt,
            row.Post.UpdatedAt,
            row.Post.PublishedAt,
            row.CommentCount
        ));
    }
}
