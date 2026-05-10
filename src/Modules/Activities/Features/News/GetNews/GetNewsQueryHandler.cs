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
            .Include(n => n.Attachments)
            .SingleOrDefaultAsync(
                n => n.Slug == request.IdOrSlug || n.Id == request.IdOrSlug,
                cancellationToken: cancellationToken
            );
        if (news is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Requested news does not exist");
        }

        var translation = news.Translations.FirstOrDefault();
        var state = JsonSerializer.Deserialize<object>(
            translation?.EditorState ?? news.EditorState
        );

        return new GetNewsResponse(
            news.Id,
            translation?.Title ?? news.Title,
            news.Slug,
            news.Thumbnail,
            translation?.Excerpt ?? news.Excerpt,
            news.PublishedDate.ToString("dd.MM.yyyy"),
            news.AuthorId,
            news.AuthorName,
            news.Category,
            news.Tags.ToList(),
            state,
            news.Attachments.Select(n => new AttachmentDto(
                    n.Id,
                    n.FileUrl,
                    n.Caption,
                    n.FileType,
                    n.ParentObjectId
                ))
                .ToList()
        );
    }
}
