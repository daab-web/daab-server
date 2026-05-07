namespace Daab.Modules.Activities.Features.News.GetAllNews;

public record GetAllNewsResponse(
    string Id,
    string Title,
    string Slug,
    string? Thumbnail,
    string? Excerpt,
    DateTime PublishedDate,
    string? AuthorId,
    string? AuthorName,
    string? Category,
    List<string> Tags
);
