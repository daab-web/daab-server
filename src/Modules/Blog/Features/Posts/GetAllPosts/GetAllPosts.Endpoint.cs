using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Models;
using Daab.SharedKernel;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.GetAllPosts;

public sealed record GetAllPostsRequest(
    [property: QueryParam] int Page = 1,
    [property: QueryParam] int PageSize = 20,
    [property: QueryParam] PostStatus? Status = null
);

public sealed class GetAllPostsEndpoint(IMediator mediator)
    : Endpoint<GetAllPostsRequest, PagedResponse<AdminPostResponse>>
{
    public override void Configure()
    {
        Get("/blog/admin/posts");
        AuthSchemes(BlogUser.Scheme);
        Roles(BlogUser.AdminRole);
    }

    public override async Task HandleAsync(GetAllPostsRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetAllPostsQuery(req.Page, req.PageSize, req.Status),
            ct
        );

        await result.Match(
            posts => Send.OkAsync(posts, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
