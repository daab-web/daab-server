using LanguageExt;

namespace Daab.Modules.Activities.Features.News.AddTranslation;

public sealed class AddTranslationCommand(AddTranslationRequest req) : MediatR.IRequest<Fin<Unit>>
{
    public string NewsId { get; } = req.NewsId;
    public string Locale { get; } = req.Locale;
    public string Title { get; } = req.Title;
    public string Excerpt { get; } = req.Excerpt;
    public string EditorState { get; } = req.EditorState;
}
