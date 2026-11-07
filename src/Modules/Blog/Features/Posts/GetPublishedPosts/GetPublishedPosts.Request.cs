using FastEndpoints;

namespace Daab.Modules.Blog.Features.Posts.GetPublishedPosts;

/// <param name="Locale">Language of the returned titles and summaries.</param>
public sealed record GetPublishedPostsRequest(
    [property: QueryParam] int Page = 1,
    [property: QueryParam] int PageSize = 10,
    [property: QueryParam] string? ScientistId = null,
    [property: QueryParam] string? Locale = null
);
