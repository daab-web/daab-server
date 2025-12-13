using Daab.Modules.Scientists.Models;
using Daab.SharedKernel;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public sealed record GetAllScientistsRequest
{
    [QueryParam]
    public int PageNumber { get; set; }

    [QueryParam]
    public int PageSize { get; set; }
}

public sealed class GetAllScientistsEndpoint(IMediator mediator)
    : Endpoint<GetAllScientistsRequest, PagedResponse<Scientist>>
{
    public override void Configure()
    {
        Get("/scientists");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAllScientistsRequest request, CancellationToken ct)
    {
        await Send.OkAsync(
            await mediator.Send(new GetAllScientistsQuery(request.PageNumber, request.PageSize), ct),
            ct
        );
    }
}
