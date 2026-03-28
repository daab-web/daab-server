using FastEndpoints;

namespace Daab.Modules.Activities.Features.News.GetNews;

public sealed record GetNewsRequest([property: QueryParam] string Locale, string IdOrSlug);
