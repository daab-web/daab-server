using Daab.Modules.Blog.Common;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Comments.AddComment;

/// <summary>Any signed-in user may comment on a published post.</summary>
public sealed class AddCommentEndpoint(IMediator mediator)
    : Endpoint<AddCommentRequest, CommentResponse>
{
    public override void Configure()
    {
        Post("/blog/posts/{postId}/comments");
        AuthSchemes(BlogUser.Scheme);
    }

    public override async Task HandleAsync(AddCommentRequest req, CancellationToken ct)
    {
        var author = BlogUser.From(User);
        if (author is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(
            new AddCommentCommand(author, req.PostId, req.Content),
            ct
        );

        await result.Match(
            comment => Send.OkAsync(comment, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
