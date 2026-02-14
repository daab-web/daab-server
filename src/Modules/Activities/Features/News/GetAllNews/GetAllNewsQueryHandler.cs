using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Features.News.GetAllNews;

public class GetAllNewsQueryHandler(ActivitiesDbContext context)
    : IRequestHandler<GetAllNewsQuery, Fin<PagedResponse<GetAllNewsResponse>>>
{
    public async Task<Fin<PagedResponse<GetAllNewsResponse>>> Handle(GetAllNewsQuery request,
        CancellationToken cancellationToken)
    {
        var news = await context.News.AsNoTracking()
            .Select(n => new GetAllNewsResponse(n.Id, n.Title, n.Slug, n.Thumbnail, n.Excerpt,
                n.PublishedDate.ToString("yyyy-MM-dd"),
                n.AuthorId, n.AuthorName, n.Category, n.Tags))
            .ToPagedResponseAsync(new PageRequest { PageNumber = request.Page, PageSize = request.PageSize },
                cancellationToken);

        return news;
    }
}