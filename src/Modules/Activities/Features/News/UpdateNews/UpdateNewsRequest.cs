using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.UpdateNews;

public sealed record UpdateNewsRequest
{
    public string? Category { get; init; }

    public List<string> Tags { get; init; } = [];

    public DateTime PublishedDate { get; init; }
}
