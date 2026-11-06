using Daab.SharedKernel;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.GetPublishedPosts;

public sealed class GetPublishedPostsEndpoint(IMediator mediator)
    : Endpoint<GetPublishedPostsRequest, PagedResponse<PostSummaryResponse>>
{
    public override void Configure()
    {
        Get("/blog/posts");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPublishedPostsRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetPublishedPostsQuery(req.Page, req.PageSize, req.ScientistId),
            ct
        );

        await result.Match(
            posts => Send.OkAsync(posts, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
