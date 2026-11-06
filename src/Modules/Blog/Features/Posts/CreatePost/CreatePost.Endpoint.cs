using Daab.Modules.Blog.Common;
using Daab.SharedKernel.Extensions;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.CreatePost;

public sealed class CreatePostEndpoint(IMediator mediator)
    : Endpoint<CreatePostRequest, CreatePostResponse>
{
    public override void Configure()
    {
        Post("/blog/posts");
        AuthSchemes(BlogUser.Scheme);
        Roles(BlogUser.ScientistRole);
        PreProcessor<LocaleCollectionPreProcessor>();
    }

    public override async Task HandleAsync(CreatePostRequest req, CancellationToken ct)
    {
        var author = BlogUser.From(User);
        if (author is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new CreatePostCommand(author, req), ct);

        await result.Match(
            post => Send.OkAsync(post, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
