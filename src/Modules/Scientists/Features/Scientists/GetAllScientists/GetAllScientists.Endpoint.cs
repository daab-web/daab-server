using Daab.SharedKernel;
using Daab.SharedKernel.Entities;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

public sealed class GetAllScientistsEndpoint(IMediator mediator, ILocaleResolver localeResolver)
    : Endpoint<GetAllScientistsRequest, PagedResponse<GetAllScientistsResponse>>
{
    public override void Configure()
    {
        Get("/scientists");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAllScientistsRequest request, CancellationToken ct)
    {
        var locale = localeResolver.Resolve();
        var response = await mediator.Send(new GetAllScientistsQuery(request, locale), ct);

        await Send.OkAsync(response, ct);
    }
}
