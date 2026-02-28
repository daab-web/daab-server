using LanguageExt;
using MediatR;

namespace Daab.Modules.Activities.Features.News.DeleteNews;

public class DeleteNewsCommand(string id) : IRequest<Fin<bool>>
{
    public string Id { get; } = id;
}
