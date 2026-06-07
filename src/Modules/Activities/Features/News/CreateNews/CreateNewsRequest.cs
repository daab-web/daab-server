using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed record CreateNewsRequest(
    string Title,
    string? AuthorId,
    string? Author,
    string? Category,
    List<string> Tags,
    DateTime PublishedDate
);
