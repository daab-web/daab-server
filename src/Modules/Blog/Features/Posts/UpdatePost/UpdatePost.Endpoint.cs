using Daab.Modules.Blog.Common;
using Daab.SharedKernel.Extensions;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.UpdatePost;

public sealed class UpdatePostEndpoint(IMediator mediator)
    : Endpoint<UpdatePostRequest, PostResponse>
{
    public override void Configure()
    {
        Put("/blog/posts/{id}");
        AuthSchemes(BlogUser.Scheme);
        Roles(BlogUser.ScientistRole);
        PreProcessor<LocaleCollectionPreProcessor>();
    }

    public override async Task HandleAsync(UpdatePostRequest req, CancellationToken ct)
    {
        var author = BlogUser.From(User);
        if (author is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new UpdatePostCommand(author, req), ct);

        await result.Match(
            post => Send.OkAsync(post, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
