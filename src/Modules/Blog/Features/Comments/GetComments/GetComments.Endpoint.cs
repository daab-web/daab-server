using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Comments.GetComments;

public sealed class GetCommentsEndpoint(IMediator mediator)
    : EndpointWithoutRequest<List<CommentResponse>>
{
    public override void Configure()
    {
        Get("/blog/posts/{postId}/comments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCommentsQuery(Route<string>("postId")!), ct);

        await result.Match(
            comments => Send.OkAsync(comments, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
