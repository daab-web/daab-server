using FastEndpoints;

namespace Daab.Modules.Activities.Features.News.GetAllNews;

public record GetAllNewsRequest(
    [property: QueryParam] string Locale,
    int Page = 1,
    int PageSize = 10
);
