using Daab.SharedKernel;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public sealed class GetAllScientistsEndpoint(IMediator mediator)
    : Endpoint<GetAllScientistsRequest, PagedResponse<GetAllScientistsResponse>>
{
    public override void Configure()
    {
        Get("/scientists");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAllScientistsRequest request, CancellationToken ct)
    {
        await Send.OkAsync(await mediator.Send(new GetAllScientistsQuery(request), ct), ct);
    }
}
