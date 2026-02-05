namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed record CreateNewsRequest(
    string Title,
    object EditorState);