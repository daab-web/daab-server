using Daab.SharedKernel;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

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
        var response = await mediator.Send(new GetAllScientistsQuery(request), ct);

        await Send.OkAsync(response, ct);
    }
}
