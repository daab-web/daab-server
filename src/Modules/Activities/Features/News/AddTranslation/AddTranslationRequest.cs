using Daab.Modules.Activities.Middlewares;

namespace Daab.Modules.Activities.Features.News.AddTranslation;

public sealed record AddTranslationRequest(
    string NewsId,
    string Locale,
    string Title,
    string Excerpt,
    string EditorState
) : ILocalized;
