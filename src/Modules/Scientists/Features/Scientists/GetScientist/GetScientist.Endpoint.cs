using Daab.SharedKernel.Entities;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.GetScientist;

public class GetScientist(IMediator mediator, ILocaleResolver localeResolver)
    : EndpointWithoutRequest<GetScientistResponse>
{
    public override void Configure()
    {
        Get("/scientists/{idOrSlug}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var idOrSlug = Route<string>("idOrSlug");
        var locale = localeResolver.Resolve();

        if (string.IsNullOrWhiteSpace(idOrSlug) || string.IsNullOrEmpty(locale))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var response = await mediator.Send(new GetScientistQuery(idOrSlug, locale), ct);

        if (response is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}
