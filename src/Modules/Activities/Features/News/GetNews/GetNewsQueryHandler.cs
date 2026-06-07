using System.Text.Json;
using Daab.Modules.Activities.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Features.News.GetNews;

public sealed class GetNewsQueryHandler(ActivitiesDbContext context)
    : IRequestHandler<GetNewsQuery, Fin<GetNewsResponse>>
{
    public async Task<Fin<GetNewsResponse>> Handle(
        GetNewsQuery request,
        CancellationToken cancellationToken
    )
    {
        var news = await context
            .News.AsNoTracking()
            .Include(n => n.Translations.Where(t => t.Locale == request.Locale))
            .SingleOrDefaultAsync(
                n => n.Slug == request.IdOrSlug || n.Id == request.IdOrSlug,
                cancellationToken: cancellationToken
            );

        if (news is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Requested news does not exist");
        }

        var translation = news.Translations.FirstOrDefault();

        if (translation is null)
        {
            return new GetNewsResponse(
                news.Id,
                "Untranslated",
                news.Slug,
                news.Thumbnail,
                "Untranslated",
                news.PublishedDate.ToString("dd.MM.yyyy"),
                news.AuthorId,
                news.AuthorName,
                news.Category,
                news.Tags.ToList(),
                null
            );
        }

        return new GetNewsResponse(
            news.Id,
            translation.Title,
            news.Slug,
            news.Thumbnail,
            translation.Excerpt,
            news.PublishedDate.ToString("dd.MM.yyyy"),
            news.AuthorId,
            news.AuthorName,
            news.Category,
            news.Tags.ToList(),
            translation.EditorState
        );
    }
}
