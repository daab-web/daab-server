using Daab.Modules.Blog.Common;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.GetMyPosts;

public sealed class GetMyPostsEndpoint(IMediator mediator)
    : EndpointWithoutRequest<List<MyPostResponse>>
{
    public override void Configure()
    {
        Get("/blog/my-posts");
        AuthSchemes(BlogUser.Scheme);
        Roles(BlogUser.ScientistRole);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var author = BlogUser.From(User);
        if (author is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new GetMyPostsQuery(author.Id), ct);

        await result.Match(
            posts => Send.OkAsync(posts, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
