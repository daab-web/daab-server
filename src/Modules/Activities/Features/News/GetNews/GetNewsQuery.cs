using LanguageExt;
using MediatR;

namespace Daab.Modules.Activities.Features.News.GetNews;

public sealed class GetNewsQuery(string idOrSlug) : IRequest<Fin<GetNewsResponse>>
{
    public string IdOrSlug { get; } = idOrSlug;
}