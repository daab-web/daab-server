using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Features.News.GetAllNews;

public class GetAllNewsQueryHandler(ActivitiesDbContext context)
    : IRequestHandler<GetAllNewsQuery, Fin<PagedResponse<GetAllNewsResponse>>>
{
    public async Task<Fin<PagedResponse<GetAllNewsResponse>>> Handle(
        GetAllNewsQuery request,
        CancellationToken cancellationToken
    )
    {
        var news = await context
            .News.Include(n => n.Translations.Where(t => t.Locale == request.Locale))
            .AsNoTracking()
            .OrderByDescending(n => n.PublishedDate)
            .ToArrayAsync(cancellationToken);

        var dtos = news.Select(n =>
        {
            var translation = n.Translations.FirstOrDefault();
            return new GetAllNewsResponse(
                n.Id,
                translation?.Title ?? n.Title,
                n.Slug,
                n.Thumbnail,
                translation?.Excerpt ?? n.Excerpt,
                n.PublishedDate,
                n.AuthorId,
                n.AuthorName,
                n.Category,
                n.Tags
            );
        });

        return PagedResponse<GetAllNewsResponse>.Create(dtos, request.Page, request.PageSize);
    }
}
