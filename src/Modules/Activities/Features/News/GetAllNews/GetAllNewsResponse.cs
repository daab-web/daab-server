namespace Daab.Modules.Activities.Features.News.GetAllNews;

public record GetAllNewsResponse(
    string Id,
    string Title,
    string Slug,
    string Thumbnail,
    string? Excerpt,
    string PublishedDate,
    string? AuthorId,
    string? AuthorName,
    string? Category,
    List<string> Tags);