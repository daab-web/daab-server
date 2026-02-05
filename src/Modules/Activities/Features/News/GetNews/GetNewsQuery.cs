using LanguageExt;
using MediatR;

namespace Daab.Modules.Activities.Features.News.GetNews;

public sealed class GetNewsQuery(string newsId) : IRequest<Fin<GetNewsResponse>>
{
    public string Id { get; } = newsId;
}