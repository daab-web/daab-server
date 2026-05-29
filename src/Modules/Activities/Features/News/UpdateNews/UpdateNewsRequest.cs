using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Daab.Modules.Activities.Features.News.UpdateNews;

public sealed record UpdateNewsRequest
{
    [FromBody]
    public required string Id { get; init; }

    [FromForm]
    public IFormFile? Thumbnail { get; init; }

    [FromBody]
    public string? Category { get; init; }

    [FromBody]
    public List<string> Tags { get; init; } = [];
}
