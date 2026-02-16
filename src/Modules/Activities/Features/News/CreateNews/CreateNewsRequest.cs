namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed record CreateNewsRequest(
    string Title,
    object EditorState,
    string Slug,
    string Thumbnail,
    string? Excerpt,
    string? AuthorId,
    string? Author,
    string? Category,
    List<string> Tags
);
