using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.GetScientist;

public class GetScientist(IMediator mediator) : EndpointWithoutRequest<GetScientistResponse>
{
    public override void Configure()
    {
        Get("/scientists/{idOrSlug}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var idOrSlug = Route<string>("idOrSlug");

        if (string.IsNullOrWhiteSpace(idOrSlug))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var response = await mediator.Send(new GetScientistQuery(idOrSlug), ct);

        if (response is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}
