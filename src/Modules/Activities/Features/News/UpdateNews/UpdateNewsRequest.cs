using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Daab.Modules.Activities.Features.News.UpdateNews;

public sealed record UpdateNewsRequest
{
    [FromBody]
    public required string Id { get; init; }

    [FromBody]
    public required string Title { get; init; }

    [FromForm]
    public IFormFile? Thumbnail { get; init; }

    [FromBody]
    public string? Excerpt { get; init; }

    [FromBody]
    public string? Category { get; init; }

    [FromBody]
    public string EditorState { get; init; } = string.Empty;

    [FromBody]
    public List<string> Tags { get; init; } = [];
}
