namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed record CreateNewsRequest(
    string Title,
    object EditorState,
    string Slug,
    string ThumbnailUri,
    string? Excerpt,
    string? AuthorId,
    string? AuthorName,
    string? Category,
    List<string> Tags
);