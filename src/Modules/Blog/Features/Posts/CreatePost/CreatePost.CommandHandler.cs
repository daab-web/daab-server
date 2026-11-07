using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Blog.Features.Posts.CreatePost;

public sealed class CreatePostCommandHandler(BlogDbContext context, BlogLocales locales)
    : IRequestHandler<CreatePostCommand, Fin<CreatePostResponse>>
{
    public async Task<Fin<CreatePostResponse>> Handle(
        CreatePostCommand request,
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

        var post = Post.Create(
            author.Id,
            author.Name,
            author.ScientistId,
            content.SourceLocale,
            input.ToContents()
        );

        await context.Posts.AddAsync(post, cancellationToken);
        var written = await context.SaveChangesAsync(cancellationToken);

        return written <= 0
            ? Error.New(
                StatusCodes.Status500InternalServerError,
                "Unable to save the post. Please try again"
            )
            : new CreatePostResponse(post.Id, post.Slug);
    }
}
