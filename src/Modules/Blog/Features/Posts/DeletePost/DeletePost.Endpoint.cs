using Daab.Modules.Blog.Common;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.DeletePost;

public sealed class DeletePostEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/blog/posts/{id}");
        AuthSchemes(BlogUser.Scheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = BlogUser.From(User);
        if (user is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new DeletePostCommand(user, Route<string>("id")!), ct);

        await result.Match(
            _ => Send.NoContentAsync(ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
