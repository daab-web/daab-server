using Daab.SharedKernel.Extensions;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Comments.TranslateComment;

/// <summary>Open to every reader: the translate button is shown to anonymous visitors too.</summary>
public sealed class TranslateCommentEndpoint(IMediator mediator)
    : Endpoint<TranslateCommentRequest, TranslateCommentResponse>
{
    public override void Configure()
    {
        Post("/blog/comments/{id}/translation");
        AllowAnonymous();
        PreProcessor<LocaleCollectionPreProcessor>();
    }

    public override async Task HandleAsync(TranslateCommentRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new TranslateCommentCommand(req.Id, req.Locale), ct);

        await result.Match(
            translation => Send.OkAsync(translation, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
