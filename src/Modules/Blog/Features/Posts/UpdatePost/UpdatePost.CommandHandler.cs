using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Blog.Features.Posts.UpdatePost;

public sealed class UpdatePostCommandHandler(BlogDbContext context, BlogLocales locales)
    : IRequestHandler<UpdatePostCommand, Fin<PostResponse>>
{
    public async Task<Fin<PostResponse>> Handle(
        UpdatePostCommand request,
        CancellationToken cancellationToken
    )
    {
        var (author, content) = request;
        IPostContent input = content;

        var complete = locales.RequireAll(
            content.SourceLocale,
            content.Translations.Select(t => t.Locale)
        );
        if (complete.IsFail)
        {
            return complete.Match(_ => Error.New("unreachable"), e => e);
        }

        var post = await context
            .Posts.Include(p => p.Translations)
            .SingleOrDefaultAsync(p => p.Id == content.Id, cancellationToken);

        if (post is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Post not found");
        }

        if (!post.IsOwnedBy(author.Id))
        {
            return Error.New(StatusCodes.Status403Forbidden, "Only the author can edit this post");
        }

        // Editing moves the post back to draft; it must pass verification again to be published.
        post.Update(content.SourceLocale, input.ToContents());
        await context.SaveChangesAsync(cancellationToken);

        return post.ToResponse();
    }
}
