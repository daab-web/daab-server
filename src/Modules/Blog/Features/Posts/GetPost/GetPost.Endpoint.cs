using Daab.Modules.Blog.Common;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.GetPost;

public sealed class GetPostEndpoint(IMediator mediator) : EndpointWithoutRequest<PostResponse>
{
    public override void Configure()
    {
        Get("/blog/posts/{idOrSlug}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Anonymous readers see published posts; authors also see their own drafts.
        var viewer = await BlogUser.FromOptionalAuthAsync(HttpContext);
        var result = await mediator.Send(new GetPostQuery(Route<string>("idOrSlug")!, viewer), ct);

        await result.Match(
            post => Send.OkAsync(post, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
