using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Activities.Features.News.AddTranslation;

public class AddTranslationEndpoint(IMediator mediator) : Endpoint<AddTranslationRequest>
{
    public override void Configure()
    {
        Post("/news/{newsId}/translations");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddTranslationRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new AddTranslationCommand(req), cancellationToken: ct);

        await result.Match(
            _ => Send.NoContentAsync(ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
