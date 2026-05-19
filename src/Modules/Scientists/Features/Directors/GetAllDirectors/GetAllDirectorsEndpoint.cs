using Daab.Modules.Scientists.Models;
using Daab.SharedKernel.Entities;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.GetAllDirectors;

public class GetAllDirectorsEndpoint(IMediator mediator, ILocaleResolver localeResolver)
    : EndpointWithMapping<EmptyRequest, GetAllDirectorsResponse, IEnumerable<Director>>
{
    public override void Configure()
    {
        Get("/directors");
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var locale = localeResolver.Resolve();
        var data = await mediator.Send(new GetAllDirectorsQuery(locale), ct);
        await Send.OkAsync(new GetAllDirectorsResponse(data), ct);
    }
}
