using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Blog.Features.Comments.GetComments;

public sealed class GetCommentsQueryHandler(BlogDbContext context)
    : IRequestHandler<GetCommentsQuery, Fin<List<CommentResponse>>>
{
    private const int MaxComments = 500;

    public async Task<Fin<List<CommentResponse>>> Handle(
        GetCommentsQuery request,
        CancellationToken cancellationToken
    )
    {
        var published = await context.Posts.AnyAsync(
            p => p.Id == request.PostId && p.Status == PostStatus.Published,
            cancellationToken
        );

        if (!published)
        {
            return Error.New(StatusCodes.Status404NotFound, "Post not found");
        }

        var comments = await context
            .Comments.AsNoTracking()
            .Include(c => c.Texts)
            .Where(c => c.PostId == request.PostId)
            .OrderBy(c => c.CreatedAt)
            .Take(MaxComments)
            .ToListAsync(cancellationToken);

        return comments.ConvertAll(c => c.ToResponse());
    }
}
