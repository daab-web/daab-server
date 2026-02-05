using LanguageExt;
using MediatR;

namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed class CreateNewsCommand(CreateNewsRequest req) : IRequest<Fin<CreateNewsResponse>>
{
    public string Title { get; } = req.Title;
    public object EditorState { get; } = req.EditorState;
}