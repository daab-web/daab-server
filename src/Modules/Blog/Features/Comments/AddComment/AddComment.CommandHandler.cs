using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Blog.Features.Comments.AddComment;

public sealed class AddCommentCommandHandler(BlogDbContext context, BlogLocales locales)
    : IRequestHandler<AddCommentCommand, Fin<CommentResponse>>
{
    public async Task<Fin<CommentResponse>> Handle(
        AddCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        var complete = locales.RequireAll(request.SourceLocale, request.Texts.Keys);
        if (complete.IsFail)
        {
            return complete.Match(_ => Error.New("unreachable"), e => e);
        }

        var published = await context.Posts.AnyAsync(
            p => p.Id == request.PostId && p.Status == PostStatus.Published,
            cancellationToken
        );

        if (!published)
        {
            return Error.New(StatusCodes.Status404NotFound, "Post not found");
        }

        var comment = Comment.Create(
            request.PostId,
            request.Author.Id,
            request.Author.Name,
            request.SourceLocale,
            request.Texts
        );

        await context.Comments.AddAsync(comment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return comment.ToResponse();
    }
}
