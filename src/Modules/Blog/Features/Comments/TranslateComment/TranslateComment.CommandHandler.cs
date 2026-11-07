using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Models;
using Daab.Modules.Blog.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Blog.Features.Comments.TranslateComment;

/// <summary>
/// Translates a comment into the requested locale. Translations are cached per locale, so each
/// comment is sent to the translation provider at most once per language.
/// </summary>
public sealed class TranslateCommentCommandHandler(
    BlogDbContext context,
    ICommentLanguageService languageService
) : IRequestHandler<TranslateCommentCommand, Fin<TranslateCommentResponse>>
{
    public async Task<Fin<TranslateCommentResponse>> Handle(
        TranslateCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        var comment = await context
            .Comments.Include(c => c.Translations.Where(t => t.Locale == request.Locale))
            .SingleOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Comment not found");
        }

        if (comment.Language == request.Locale)
        {
            return new TranslateCommentResponse(
                comment.Id,
                request.Locale,
                comment.Content,
                comment.Language
            );
        }

        if (comment.Translations.FirstOrDefault() is { } cached)
        {
            return new TranslateCommentResponse(
                comment.Id,
                request.Locale,
                cached.Content,
                comment.Language
            );
        }

        var translated = await languageService.TranslateAsync(
            comment.Content,
            request.Locale,
            cancellationToken
        );

        if (translated.IsFail)
        {
            return Error.New(
                StatusCodes.Status503ServiceUnavailable,
                "Translation is unavailable right now. Please try again later"
            );
        }

        var content = translated.Match(text => text, _ => comment.Content);

        context.CommentTranslations.Add(
            new CommentTranslation
            {
                CommentId = comment.Id,
                Locale = request.Locale,
                Content = content,
            }
        );

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // A concurrent request cached the same translation first; the result is equivalent.
        }

        return new TranslateCommentResponse(comment.Id, request.Locale, content, comment.Language);
    }
}
