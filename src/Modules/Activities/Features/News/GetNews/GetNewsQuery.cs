using LanguageExt;
using MediatR;

namespace Daab.Modules.Activities.Features.News.GetNews;

public sealed class GetNewsQuery(string idOrSlug, string locale) : IRequest<Fin<GetNewsResponse>>
{
    public string IdOrSlug { get; } = idOrSlug;
    public string Locale { get; } = locale;
}
