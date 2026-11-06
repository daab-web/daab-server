using Daab.Modules.Blog.Common;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.SubmitPost;

public sealed class SubmitPostEndpoint(IMediator mediator)
    : EndpointWithoutRequest<SubmitPostResponse>
{
    public override void Configure()
    {
        Post("/blog/posts/{id}/submit");
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

        var result = await mediator.Send(new SubmitPostCommand(author, Route<string>("id")!), ct);

        await result.Match(
            post => Send.OkAsync(post, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
