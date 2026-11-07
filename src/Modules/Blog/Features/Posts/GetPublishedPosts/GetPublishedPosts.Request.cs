using FastEndpoints;

namespace Daab.Modules.Blog.Features.Posts.GetPublishedPosts;

public sealed record GetPublishedPostsRequest(
    [property: QueryParam] int Page = 1,
    [property: QueryParam] int PageSize = 10,
    [property: QueryParam] string? ScientistId = null
);
